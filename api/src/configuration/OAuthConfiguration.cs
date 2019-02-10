namespace BasicApi.Configuration
{
    /*
     * OAuth settings
     */
    public class OAuthConfiguration
    {
        public string Authority {get; set;}

        public string ClientId {get; set;}

        public string ClientSecret {get; set;}

        public int DefaultTokenCacheMinutes {get; set;}
    }
}
