namespace SampleApi.Host.Plumbing.Claims
{
    /*
     * API claims used for authorization
     */
    public class CoreApiClaims
    {
        // The immutable user id from the access token, which may exist in the API's database
        public string UserId { get; set; }

        public string ClientId { get; set; }

        public string[] Scopes { get; set; }

        // Details from the Central User Data for given name, family name and email
        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string Email { get; set; }

        /*
         * Set token claims after introspection
         */
        public void SetTokenInfo(string userId, string clientId, string[] scopes)
        {
            this.UserId = userId;
            this.ClientId = clientId;
            this.Scopes = scopes;
        }

        /*
         * Set fields after receiving OAuth user info data
         */
        public void SetCentralUserInfo(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}
