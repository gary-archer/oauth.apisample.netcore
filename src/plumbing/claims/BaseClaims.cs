namespace SampleApi.Plumbing.Claims
{
    /*
     * Base claims always received in the access token
     */
    public class BaseClaims
    {
        public BaseClaims(string subject, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        public string Subject { get; private set; }

        public string[] Scopes { get; private set; }

        public int Expiry { get; private set; }
    }
}
