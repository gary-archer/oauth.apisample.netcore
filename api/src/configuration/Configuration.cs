namespace BasicApi.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Framework.Configuration;

    /// <summary>
    /// A class to manage our JSON configuration as an object
    /// </summary>
    public class Configuration
    {
        public ApplicationConfiguration App {get; private set;}
        public OAuthConfiguration OAuth {get; private set;}

        /// <summary>
        /// A helper method to load this custom configuration section
        /// </summary>
        /// <param name="configuration">The .Net configuration</param>
        /// <returns>An object to represent our custom JSON configuration</returns>
        public static Configuration Load(IConfiguration configuration)
        {
            var appConfig = new ApplicationConfiguration();
            configuration.GetSection("application").Bind(appConfig);

            var oauthConfig = new OAuthConfiguration();
            configuration.GetSection("oauth").Bind(oauthConfig);

            return new Configuration()
            {
                App = appConfig,
                OAuth = oauthConfig
            };
        }
    }
}