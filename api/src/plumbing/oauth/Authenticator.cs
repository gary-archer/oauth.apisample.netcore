namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Threading.Tasks;
    using BasicApi.Configuration;

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
        public Authenticator(OAuthConfiguration configuration, IssuerMetadata metadata) {
            this.configuration = configuration;
            this.metadata = metadata;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task<Tuple<bool, int>> ValidateTokenAndSetClaims(string accessToken, ApiClaims claims)
        {
            claims.SetTokenInfo("7890235", "7890235790423", new string[] {"openid", "email", "profile"});
            return new Tuple<bool, int>(true, 0);
        }

        /*
         * The entry point for user lookup
         */
        public async Task<bool> SetCentralUserInfoClaims(string accessToken, ApiClaims claims) {

            claims.SetCentralUserInfo("Guest", "User", "guestuser@authguidance.com");
            return true;
        }
    }
}
