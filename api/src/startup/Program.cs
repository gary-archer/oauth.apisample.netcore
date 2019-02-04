namespace BasicApi.Startup
{
    using System;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using BasicApi.Configuration;
    using BasicApi.Plumbing.Utilities;

    /*
     * Our entry point class
     */
    public class Program
    {
        /*
         * Our entry point method
         */
        public static void Main(string[] args)
        {
            // First handle configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Use the configuration to build and run the web host
            BuildWebHost(args, config).Run();
        }

        /* 
         * Create a web host to listen for HTTP requests
         */
        private static IWebHost BuildWebHost(string[] args, IConfigurationRoot configuration)
        {
            // Read our custom configuration
            var appConfig = ApplicationConfiguration.Load(configuration);
            var webUrl = new Uri(appConfig.TrustedOrigins[0]);

            // Create the web host to listen over SSL
            return new WebHostBuilder()
                .UseConfiguration(configuration)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole().AddFilter(LoggingSetup.Filter);
                })
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, webUrl.Port, listenOptions =>
                    {   
                        listenOptions.UseHttps($"./certs/{appConfig.SslCertificateFileName}", appConfig.SslCertificatePassword);
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
        }
    }
}
