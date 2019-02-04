namespace BasicApi.Configuration
{
    using Microsoft.Extensions.Configuration;

    /*
     * OAuth settings
     */
    public class OAuthConfiguration
    {
        /*
         * A helper method to load this custom configuration section
         */
        public static OAuthConfiguration Load(IConfiguration configuration)
        {
            var oauthConfig = new OAuthConfiguration();
            configuration.GetSection("oauth").Bind(oauthConfig);
            return oauthConfig;
        }

        public string Authority {get; set;}

        public string ClientId {get; set;}

        public string ClientSecret {get; set;}

        public int DefaultTokenCacheMinutes {get; set;}
    }
}
