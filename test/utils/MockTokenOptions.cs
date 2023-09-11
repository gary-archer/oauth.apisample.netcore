namespace SampleApi.Test.Utils
{
    /*
     * Options to use when issuing mock access tokens
     */
    public sealed class MockTokenOptions
    {
        public MockTokenOptions()
        {
            this.Issuer = "https://login.authsamples-dev.com";
            this.Audience = "api.mycompany.com";
            this.Scope = "openid profile investments";
            this.Role = string.Empty;
            this.ExpiryMinutes = 15;
            this.Subject = string.Empty;
            this.ManagerId = string.Empty;
        }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Scope { get; set; }

        public string Role { get; set; }

        public int ExpiryMinutes { get; set; }

        public string Subject { get; private set; }

        public string ManagerId { get; private set; }

        /*
         * Test with the user identities for the standard user
         */
        public void UseStandardUser()
        {
            this.Subject = "a6b404b1-98af-41a2-8e7f-e4061dc0bf86";
            this.ManagerId = "10345";
            this.Role = "user";
        }

        /*
         * Test with the user identities for the admin user
         */
        public void UseAdminUser()
        {
            this.Subject = "77a97e5b-b748-45e5-bb6f-658e85b2df91";
            this.ManagerId = "20116";
            this.Role = "admin";
        }
    }
}
