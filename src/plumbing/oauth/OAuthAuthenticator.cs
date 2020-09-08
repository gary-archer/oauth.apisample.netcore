namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;

    /*
     * The class from which OAuth calls are initiated
     */
    internal sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly DiscoveryDocumentResponse metadata;
        private readonly Func<HttpClientHandler> proxyFactory;
        private readonly LogEntry logEntry;

        public OAuthAuthenticator(
            OAuthConfiguration configuration,
            IssuerMetadata issuer,
            Func<HttpClientHandler> proxyFactory,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.metadata = issuer.Metadata;
            this.proxyFactory = proxyFactory;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task ValidateTokenAndGetClaims(string accessToken, HttpRequest httpRequest, CoreApiClaims claims)
        {
            // Create a child log entry for authentication related work
            // This ensures that any errors and performances in this area are reported separately to business logic
            var authorizationLogEntry = this.logEntry.CreateChild("Authorizer");

            // First validate the token and get token claims, using introspection if supported
            if (this.metadata.IntrospectionEndpoint != null)
            {
                await this.IntrospectTokenAndGetTokenClaims(accessToken, claims);
            }
            else
            {
                await this.ValidateTokenInMemoryAndGetTokenClaims(accessToken, claims);
            }

            // It then adds user info claims
            await this.GetUserInfoClaims(accessToken, claims);

            // Finish logging here, and note that on exception our logging disposes the child
            authorizationLogEntry.Dispose();
        }

        /*
         * Validate the access token via introspection and populate claims
         */
        private async Task IntrospectTokenAndGetTokenClaims(string accessToken, CoreApiClaims claims)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                try
                {
                    using (var client = new HttpClient(this.proxyFactory()))
                    {
                        // Send the request
                        var request = new TokenIntrospectionRequest
                        {
                            Address = this.metadata.IntrospectionEndpoint,
                            ClientId = this.configuration.ClientId,
                            ClientSecret = this.configuration.ClientSecret,
                            Token = accessToken,
                        };
                        var response = await client.IntrospectTokenAsync(request);

                        // Handle errors
                        if (response.IsError)
                        {
                            throw ErrorUtils.FromIntrospectionError(response, this.metadata.IntrospectionEndpoint);
                        }

                        // Handle invalid or expired tokens
                        if (!response.IsActive)
                        {
                            throw ErrorFactory.CreateClient401Error("Access token is expired and failed introspection");
                        }

                        // Get token claims
                        string subject = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Subject);
                        string clientId = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.ClientId);
                        string scope = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Scope);
                        int expiry = this.GetIntegerClaim((name) => response.TryGet(name), JwtClaimTypes.Expiration);
                        claims.SetTokenInfo(subject, clientId, scope.Split(' '), expiry);
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromIntrospectionError(ex, this.metadata.IntrospectionEndpoint);
                }
            }
        }

        /*
         * Validate the access token in memory via the token signing public key
         */
        private async Task ValidateTokenInMemoryAndGetTokenClaims(string accessToken, CoreApiClaims claims)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                // Next get the token signing public key
                var keys = await this.GetTokenSigningPublicKeys();

                // Next validate the token
                var principal = this.ValidateJsonWebToken(accessToken, keys);

                // Get token claims
                string subject = this.GetStringClaim((name) => principal.FindFirstValue(name), "username");
                string clientId = this.GetStringClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.ClientId);
                string scope = this.GetStringClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Scope);
                int expiry = this.GetIntegerClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Expiration);
                claims.SetTokenInfo(subject, clientId, scope.Split(' '), expiry);
            }
        }

        /*
         * Get the keys from the JWKS endpoint
         */
        private async Task<string> GetTokenSigningPublicKeys()
        {
            using (this.logEntry.CreatePerformanceBreakdown("getTokenSigningPublicKey"))
            {
                try
                {
                    using (var client = new HttpClient(this.proxyFactory()))
                    {
                        // Make the HTTPS request
                        var response = await client.GetJsonWebKeySetAsync(this.metadata.JwksUri);
                        if (response.IsError)
                        {
                            throw ErrorUtils.FromTokenSigningKeysDownloadError(response, this.metadata.JwksUri);
                        }

                        // Return the JSON data
                        return response.Json.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromTokenSigningKeysDownloadError(ex, this.metadata.JwksUri);
                }
            }
        }

        /*
        * Do the work of verifying the access token
        */
        private ClaimsPrincipal ValidateJsonWebToken(string accessToken, string keys)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateJsonWebToken"))
            {
                try
                {
                    // Set parameters, and Cognito does not provide an audience claim in access tokens
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = this.configuration.Authority,
                        ValidateIssuer = true,
                        IssuerSigningKeys = new JsonWebKeySet(keys).Keys,
                        ValidateAudience = false,
                    };

                    // Do the technical validation, including checking the digital signature via the public key
                    var handler = new JwtSecurityTokenHandler();
                    SecurityToken result;
                    return handler.ValidateToken(accessToken, tokenValidationParameters, out result);
                }
                catch (Exception ex)
                {
                    // Handle failures and log the error details
                    var details = $"JWT verification failed: ${ex.Message}";
                    throw ErrorFactory.CreateClient401Error(details);
                }
            }
        }

        /*
         * Perform OAuth user info lookup
         */
        private async Task GetUserInfoClaims(string accessToken, CoreApiClaims claims)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
                try
                {
                    using (var client = new HttpClient(this.proxyFactory()))
                    {
                        // Send the request
                        var request = new UserInfoRequest
                        {
                            Address = this.metadata.UserInfoEndpoint,
                            Token = accessToken,
                        };
                        var response = await client.GetUserInfoAsync(request);

                        // Handle errors
                        if (response.IsError)
                        {
                            // Handle a race condition where the access token expires during user info lookup
                            if (response.HttpStatusCode == HttpStatusCode.Unauthorized)
                            {
                                throw ErrorFactory.CreateClient401Error("Access token is expired and failed user info lookup");
                            }

                            // Handle technical errors
                            throw ErrorUtils.FromUserInfoError(response, this.metadata.UserInfoEndpoint);
                        }

                        // Get token claims and use the immutable user id as the subject claim
                        string givenName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.GivenName);
                        string familyName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.FamilyName);
                        string email = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Email);
                        claims.SetUserInfo(givenName, familyName, email);
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.metadata.UserInfoEndpoint);
                }
            }
        }

        /*
         * A helper to get a string claim
         */
        private string GetStringClaim(Func<string, string> callback, string name)
        {
            var value = callback(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        /*
         * A helper to get an integer claim
         */
        private int GetIntegerClaim(Func<string, string> callback, string name)
        {
            var value = callback(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
    }
}
