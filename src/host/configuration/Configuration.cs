namespace SampleApi.Host.Configuration
{
    using Framework.Configuration;
    using Microsoft.Extensions.Configuration;

    /*
     * A class to manage our JSON configuration as an object
     */
    public class Configuration
    {
        public ApplicationConfiguration App { get; private set; }

        public OAuthConfiguration OAuth { get; private set; }

        /*
         * A helper method to load this custom configuration section
         */
        public static Configuration Load(IConfiguration configuration)
        {
            var appConfig = new ApplicationConfiguration();
            configuration.GetSection("application").Bind(appConfig);

            var oauthConfig = new OAuthConfiguration();
            configuration.GetSection("oauth").Bind(oauthConfig);

            return new Configuration()
            {
                App = appConfig,
                OAuth = oauthConfig,
            };
        }
    }
}