namespace SampleApi.Plumbing.Configuration
{
    /*
     * OAuth configuration settings
     */
    public sealed class OAuthConfiguration
    {
        public string Authority { get; set; }

        public string RequiredScope { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
