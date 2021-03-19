namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
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
        public async Task<TokenClaims> ValidateTokenAsync(string accessToken)
        {
            // Validate the token and read token claims
            if (!string.IsNullOrWhiteSpace(this.metadata.IntrospectionEndpoint) &&
                !string.IsNullOrWhiteSpace(this.configuration.ClientId) &&
                !string.IsNullOrWhiteSpace(this.configuration.ClientId))
            {
                // Use introspection if we can
                return await this.IntrospectTokenAndGetTokenClaimsAsync(accessToken);
            }
            else
            {
                // Use in memory validation otherwise
                return await this.ValidateTokenInMemoryAndGetTokenClaimsAsync(accessToken);
            }
        }

        /*
         * Perform OAuth user info lookup
         */
        public async Task<UserInfoClaims> GetUserInfoAsync(string accessToken)
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
                            // Handle technical errors
                            if (response.Exception != null)
                            {
                                throw ErrorUtils.FromUserInfoError(response.Exception, this.metadata.UserInfoEndpoint);
                            }
                            else
                            {
                                throw ErrorUtils.FromUserInfoError(response, this.metadata.UserInfoEndpoint);
                            }
                        }

                        // Get token claims and use the immutable user id as the subject claim
                        string givenName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.GivenName);
                        string familyName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.FamilyName);
                        string email = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Email);
                        return new UserInfoClaims(givenName, familyName, email);
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.metadata.UserInfoEndpoint);
                }
            }
        }

        /*
         * Validate the access token via introspection and populate claims
         */
        private async Task<TokenClaims> IntrospectTokenAndGetTokenClaimsAsync(string accessToken)
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

                        // Handle errors
                        var response = await client.IntrospectTokenAsync(request);
                        if (response.IsError)
                        {
                            if (response.Exception != null)
                            {
                                throw ErrorUtils.FromIntrospectionError(response.Exception, this.metadata.IntrospectionEndpoint);
                            }
                            else
                            {
                                throw ErrorUtils.FromIntrospectionError(response, this.metadata.IntrospectionEndpoint);
                            }
                        }

                        // Handle invalid or expired tokens
                        if (!response.IsActive)
                        {
                            throw ErrorFactory.CreateClient401Error("Access token is expired and failed introspection");
                        }

                        // Get token claims
                        string subject = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Subject);
                        string[] scopes = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Scope).Split(' ');
                        int expiry = this.GetIntegerClaim((name) => response.TryGet(name), JwtClaimTypes.Expiration);

                        // Make sure the token is for this API
                        this.VerifyScopes(scopes);

                        // Update token claims
                        return new TokenClaims(subject, scopes, expiry);
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
        private async Task<TokenClaims> ValidateTokenInMemoryAndGetTokenClaimsAsync(string accessToken)
        {
            using (var breakdown = this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                // Next get the token signing public key
                var keys = await this.GetTokenSigningPublicKeysAsync(breakdown);

                // Next validate the token
                var principal = this.ValidateJsonWebToken(accessToken, keys, breakdown);

                // Get token claims, and note that Microsoft move the subject claim to a username field
                string subject = this.GetStringClaim((name) => principal.FindFirstValue(name), "username");
                string[] scopes = this.GetStringClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Scope).Split(' ');
                int expiry = this.GetIntegerClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Expiration);

                // Make sure the token is for this API
                this.VerifyScopes(scopes);

                // Update token claims
                return new TokenClaims(subject, scopes, expiry);
            }
        }

        /*
         * Get the keys from the JWKS endpoint
         */
        private async Task<string> GetTokenSigningPublicKeysAsync(IPerformanceBreakdown breakdown)
        {
            using (breakdown.CreateChild("getTokenSigningPublicKey"))
            {
                try
                {
                    using (var client = new HttpClient(this.proxyFactory()))
                    {
                        // Make the HTTPS request
                        var response = await client.GetJsonWebKeySetAsync(this.metadata.JwksUri);
                        if (response.IsError)
                        {
                            if (response.Exception != null)
                            {
                                throw ErrorUtils.FromTokenSigningKeysDownloadError(response.Exception, this.metadata.JwksUri);
                            }
                            else
                            {
                                throw ErrorUtils.FromTokenSigningKeysDownloadError(response, this.metadata.JwksUri);
                            }
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
        private ClaimsPrincipal ValidateJsonWebToken(string accessToken, string keys, IPerformanceBreakdown breakdown)
        {
            using (breakdown.CreateChild("validateJsonWebToken"))
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
                        ValidAlgorithms = new[] { "RS256" },
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
        * Make sure the token is for this API
        */
        private void VerifyScopes(string[] scopes)
        {
            if (!scopes.ToList().Exists((s) => s == this.configuration.RequiredScope))
            {
                throw ErrorFactory.CreateClient401Error("Access token does not have a valid scope for this API");
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
