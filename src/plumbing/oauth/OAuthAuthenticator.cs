namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Http;
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
            await this.GetCentralUserInfoClaims(accessToken, claims);

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

                    // Get token claims and use the immutable user id as the subject claim
                    string subject = this.GetIntrospectionClaim(response, "uid");
                    string clientId = this.GetIntrospectionClaim(response, JwtClaimTypes.ClientId);
                    string scope = this.GetIntrospectionClaim(response, JwtClaimTypes.Scope);
                    int expiry = Convert.ToInt32(this.GetIntrospectionClaim(response, JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);
                    claims.SetTokenInfo(subject, clientId, scope.Split(' '), expiry);
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
                // First decode the JWT
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);
                
                // Next get the token signing public key
                var publicKey = await this.GetTokenSigningPublicKey(jwt.Header.Kid);

                // Next validate the token
                await this.ValidateJsonWebToken(jwt, publicKey);

                // Next read token claims
            }
        }

        /*
         * Get the public key with which our access token is signed
         */
        private async Task<string> GetTokenSigningPublicKey(string keyIdentifier)
        {
            using (this.logEntry.CreatePerformanceBreakdown("getTokenSigningPublicKeyeToken"))
            {
            }
        }

        /*
        * Do the work of verifying the access token
        */
        private async Task ValidateJsonWebToken(JwtSecurityToken jwt, string publicKey)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateJsonWebToken"))
            {
            }
        }

        /*
         * User info lookup
         */
        private async Task GetCentralUserInfoClaims(string accessToken, CoreApiClaims claims)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
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
                    string givenName = this.GetUserInfoClaim(response, JwtClaimTypes.GivenName);
                    string familyName = this.GetUserInfoClaim(response, JwtClaimTypes.FamilyName);
                    string email = this.GetUserInfoClaim(response, JwtClaimTypes.Email);
                    claims.SetCentralUserInfo(givenName, familyName, email);
                }
            }
        }

        /*
         * A helper to check the expected introspection claims are present
         */
        private string GetIntrospectionClaim(TokenIntrospectionResponse response, string name)
        {
            var value = response.TryGet(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        /*
         * A helper to check the expected user info claims are present
         */
        private string GetUserInfoClaim(UserInfoResponse response, string name)
        {
            var value = response.TryGet(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }
    }
}
