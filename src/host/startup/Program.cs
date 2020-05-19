namespace SampleApi.Host.Startup
{
    using System;
    using System.IO;
    using System.Net;
    using Framework.Api.Base.Logging;
    using Microsoft.AspNetCore.Hosting;
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
            var loggerFactory = LoggerFactoryBuilder.Create();

            try
            {
                // Build and run the web host
                BuildWebHost(loggerFactory).Run();
            }
            catch (Exception ex)
            {
                // Report startup errors
                loggerFactory.LogStartupError(ex);
            }
        }

        /*
         * Build a host to handle HTTP requests
         */
        private static IWebHost BuildWebHost(ILoggerFactory loggerFactory)
        {
            // Load the configuration file
            var jsonConfig = Configuration.Load("./api.config.json");

            // Build the web host
            var webUrl = new Uri(jsonConfig.Api.TrustedOrigins[0]);
            return new WebHostBuilder()

                // Inject early objects in the container here
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILoggerFactory>(loggerFactory);
                    services.AddSingleton(jsonConfig);
                })

                // Configure logging behaviour for both production and developer trace logging
                .ConfigureLogging(loggingBuilder =>
                {
                    loggerFactory.Configure(loggingBuilder, jsonConfig.Logging);
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
