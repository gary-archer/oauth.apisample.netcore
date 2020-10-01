namespace SampleApi.Plumbing.Claims
{
    /*
     * API claims used for authorization
     */
    public class CoreApiClaims
    {
        // Token claims
        public string Subject { get; set; }

        public string ClientId { get; set; }

        public string[] Scopes { get; set; }

        public int Expiry { get; set; }

        // Data from the OAuth user info endpoint
        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string Email { get; set; }

        // The database primary key from the API's own database
        public string UserDatabaseId { get; set; }

        /*
         * Set token claims after introspection
         */
        public void SetTokenInfo(string subject, string clientId, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.ClientId = clientId;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        /*
         * Set fields after receiving OAuth user info data
         */
        public void SetUserInfo(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}
