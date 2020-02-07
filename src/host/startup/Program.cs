namespace SampleApi.Host.Startup
{
    using System;
    using System.IO;
    using System.Net;
    using Framework.Api.Base.Middleware;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Configuration;

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
                .AddJsonFile("api.config.json")
                .Build();

            try
            {
                // Build and run the web host
                BuildWebHost(config).Run();
            }
            catch (Exception ex)
            {
                // Report startup errors
                var handler = new UnhandledExceptionMiddleware();
                handler.HandleStartupException(ex);
            }
        }

        /*
         * Build a host to handle HTTP requests
         */
        private static IWebHost BuildWebHost(IConfigurationRoot configurationRoot)
        {
            // Load the configuration file
            var jsonConfig = Configuration.Load("./api.config.json");

            // Build the web host
            var webUrl = new Uri(jsonConfig.Api.TrustedOrigins[0]);
            return new WebHostBuilder()

                .UseConfiguration(configurationRoot)

                // Enable our JSON configuration object to be injected into other classes
                .ConfigureServices(services =>
                {
                    services.AddSingleton(jsonConfig);
                })

                // Configure logging behaviour to work for both development and production
                .ConfigureLogging(loggingBuilder =>
                {
                    var factory = new Framework.Api.Base.Logging.LoggerFactory();
                    factory.Configure(loggingBuilder, jsonConfig.Framework.Logging);
                })

                // Configure the Kestrel web server to listen over SSL
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, webUrl.Port, listenOptions =>
                    {
                        listenOptions.UseHttps($"./certs/{jsonConfig.Api.SslCertificateFileName}", jsonConfig.Api.SslCertificatePassword);
                    });
                })

                // Serve web content from the root folder
                .UseContentRoot(Directory.GetCurrentDirectory())

                // Do main configuration in the startup class
                .UseStartup<Startup>()
                .Build();
        }
    }
}
