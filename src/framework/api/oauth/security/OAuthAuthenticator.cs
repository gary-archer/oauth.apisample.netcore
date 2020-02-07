namespace Framework.Api.OAuth.Security
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Framework.Api.Base.Claims;
    using Framework.Api.Base.Errors;
    using Framework.Api.OAuth.Configuration;
    using Framework.Api.OAuth.Errors;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Http;

    /*
     * The class from which OAuth calls are initiated
     */
    public sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly DiscoveryDocumentResponse metadata;
        private readonly Func<HttpClientHandler> proxyFactory;

        public OAuthAuthenticator(OAuthConfiguration configuration, IssuerMetadata issuer, Func<HttpClientHandler> proxyFactory)
        {
            this.configuration = configuration;
            this.metadata = issuer.Metadata;
            this.proxyFactory = proxyFactory;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task<int> AuthenticateAndSetClaims(string accessToken, HttpRequest httpRequest, CoreApiClaims claims)
        {
            // Our implementation introspects the token to get token claims
            var expiry = await this.IntrospectTokenAndSetTokenClaims(accessToken, claims);

            // It then adds user info claims
            await this.SetCentralUserInfoClaims(accessToken, claims);

            // Return the expiry, used for claims caching
            return expiry;
        }

        /*
         * Introspection processing
         */
        private async Task<int> IntrospectTokenAndSetTokenClaims(string accessToken, CoreApiClaims claims)
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
                    var handler = new OAuthErrorUtils();
                    throw handler.FromIntrospectionError(response, this.metadata.IntrospectionEndpoint);
                }

                // Handle invalid or expired tokens
                if (!response.IsActive)
                {
                    throw ClientError.Create401("Access token is expired and failed introspection");
                }

                // Get token claims and use the immutable user id as the subject claim
                string userId = this.GetIntrospectionClaim(response, "uid");
                string clientId = this.GetIntrospectionClaim(response, JwtClaimTypes.ClientId);
                string scope = this.GetIntrospectionClaim(response, JwtClaimTypes.Scope);
                claims.SetTokenInfo(userId, clientId, scope.Split(' '));

                // Indicate success and return the token expiry
                return Convert.ToInt32(this.GetIntrospectionClaim(response, JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);
            }
        }

        /*
         * User info lookup
         */
        private async Task SetCentralUserInfoClaims(string accessToken, CoreApiClaims claims)
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
                        throw ClientError.Create401("Access token is expired and failed user info lookup");
                    }

                    // Handle technical errors
                    var handler = new OAuthErrorUtils();
                    throw handler.FromUserInfoError(response, this.metadata.UserInfoEndpoint);
                }

                // Get token claims and use the immutable user id as the subject claim
                string givenName = this.GetUserInfoClaim(response, JwtClaimTypes.GivenName);
                string familyName = this.GetUserInfoClaim(response, JwtClaimTypes.FamilyName);
                string email = this.GetUserInfoClaim(response, JwtClaimTypes.Email);
                claims.SetCentralUserInfo(givenName, familyName, email);
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
                var handler = new OAuthErrorUtils();
                throw handler.FromMissingClaim(name);
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
                var handler = new OAuthErrorUtils();
                throw handler.FromMissingClaim(name);
            }

            return value;
        }
    }
}
