namespace FinalApi.Host.Startup
{
    using System;
    using System.Net;
    using FinalApi.Host.Configuration;
    using FinalApi.Plumbing.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * Our entry point class
     */
    public static class Program
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
                BuildWebHost(loggerFactory).Build().Run();
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
        private static IWebHostBuilder BuildWebHost(ILoggerFactory loggerFactory)
        {
            // Load the configuration file
            var configuration = Configuration.LoadAsync("./api.config.json").Result;

            // Build the web host
            return new WebHostBuilder()

                // Inject early objects in the container here
                .ConfigureServices(services =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(configuration);
                })

                // Configure logging behaviour for both production and developer trace logging
                .ConfigureLogging(loggingBuilder =>
                {
                    loggerFactory.Configure(loggingBuilder, configuration.Logging);
                })

                // Configure the Kestrel web server
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Api.Port, listenOptions =>
                    {
                        if (!string.IsNullOrWhiteSpace(configuration.Api.SslCertificateFileName) &&
                            !string.IsNullOrWhiteSpace(configuration.Api.SslCertificatePassword))
                        {
                            // Listen over HTTPS if we configure certificate details, or HTTP otherwise
                            listenOptions.UseHttps(
                                configuration.Api.SslCertificateFileName,
                                configuration.Api.SslCertificatePassword);
                        }
                    });
                })

                // Do main configuration in the startup class
                .UseStartup<Startup>();
        }
    }
}
