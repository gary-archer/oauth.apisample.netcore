namespace SampleApi.Plumbing.Claims
{
    /*
     * Claims received in the access token
     */
    public class TokenClaims
    {
        public TokenClaims(string subject, string clientId, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.ClientId = clientId;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        public string Subject { get; set; }

        public string ClientId { get; set; }

        public string[] Scopes { get; set; }

        public int Expiry { get; set; }
    }
}
