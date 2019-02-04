namespace BasicApi.Configuration
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    /*
     * Application specific settings
     */
    public class ApplicationConfiguration
    {
        /*
         * A helper method to load this custom configuration section
         */
        public static ApplicationConfiguration Load(IConfiguration configuration)
        {
            var appConfig = new ApplicationConfiguration();
            configuration.GetSection("application").Bind(appConfig);
            return appConfig;
        }

        public string SslCertificateFileName {get; set;}

        public string SslCertificatePassword {get; set;}

        public string ProxyUrl {get; set;}

        public bool useProxy {get; set;}

        public List<string> TrustedOrigins {get; set;}
    }
}
