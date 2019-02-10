namespace BasicApi.Startup
{
    using System;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
            var jsonConfig = Configuration.Load(configuration);
            var webUrl = new Uri(jsonConfig.App.TrustedOrigins[0]);

            return new WebHostBuilder()
                .UseConfiguration(configuration)
                
                // Register our JSON configuration
                .ConfigureServices(s => s.AddSingleton(jsonConfig))
                
                // Configure a custom logging filter
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole().AddFilter(LoggingSetup.Filter);
                })

                // Configure the Kestrel web server to listen over SSL
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, webUrl.Port, listenOptions =>
                    {   
                        listenOptions.UseHttps($"./certs/{jsonConfig.App.SslCertificateFileName}", jsonConfig.App.SslCertificatePassword);
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
        }
    }
}
