namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using BasicApi.Configuration;
    using BasicApi.Plumbing.Errors;

    /*
     * The class from which OAuth calls are initiated
     */
    public class Authenticator
    {
        /*
         * The injected dependencies
         */
        private readonly OAuthConfiguration configuration;
        private readonly IssuerMetadata metadata;

        /*
        * Receive dependencies
        */
        public Authenticator(OAuthConfiguration configuration, IssuerMetadata metadata)
        {
            this.configuration = configuration;
            this.metadata = metadata;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task<Tuple<bool, int>> ValidateTokenAndSetClaims(string accessToken, ApiClaims claims)
        {
            using (var client = new IntrospectionClient(
                this.metadata.Metadata.IntrospectionEndpoint,
                this.configuration.ClientId,
                this.configuration.ClientSecret,
                this.metadata.ProxyHandler))
            {
                // Send the access token
                var request = new IntrospectionRequest()
                {
                    Token = accessToken
                };
                var response = await client.SendAsync(request);
                
                // Handle errors
                if (response.IsError)
                {
                    var handler = new ErrorHandler();
                    throw handler.FromIntrospectionError(response, this.metadata.Metadata.IntrospectionEndpoint);
                }

                // Handle invalid or expired tokens
                if (!response.IsActive)
                {
                    return Tuple.Create(false, 0);
                }

                // Get token claims and use the immutable user id as the subject claim
                string userId = this.GetIntrospectionClaim(response, "uid");
                string clientId = this.GetIntrospectionClaim(response, JwtClaimTypes.ClientId);
                string scope = this.GetIntrospectionClaim(response, JwtClaimTypes.Scope);
                claims.SetTokenInfo(userId, clientId, scope.Split(' '));

                // Return a success result
                var expiry = Convert.ToInt32(this.GetIntrospectionClaim(response, "exp"), CultureInfo.InvariantCulture);
                return Tuple.Create(true, expiry);
            }
        }

        /*
         * The entry point for user lookup
         */
        public async Task<bool> SetCentralUserInfoClaims(string accessToken, ApiClaims claims)
        {

            claims.SetCentralUserInfo("Guest", "User", "guestuser@authguidance.com");
            return true;
        }

        /*
         * A helper to check the expected claims are present
         */
        private string GetIntrospectionClaim(IntrospectionResponse response, string name)
        {
            var value = response.TryGet(name);
            if (value == null)
            {
                var handler = new ErrorHandler();
                throw handler.FromMissingClaim(name);
            }

            return value;
        }
    }
}
