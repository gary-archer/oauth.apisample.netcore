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
    using BasicApi.Errors;
    using BasicApi.Utilities;

    /// <summary>
    /// Our entry point class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Our entry point method
        /// </summary>
        /// <param name="args">Startup arguments</param>
        public static void Main(string[] args)
        {
            // First handle configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            try {

                // Build and run the web host
                BuildWebHost(config).Run();
            }
            catch(Exception ex) {

                // Report startup errors
                var handler = new UnhandledExceptionHandler(LoggingHelper.CreateStartupLogger());
                handler.HandleStartupException(ex);
            }
        }

        /// <summary>
        /// Build a host to handle HTTP requests
        /// </summary>
        /// <param name="configurationRoot">The .Net core configuration</param>
        /// <returns>A built web host</returns>
        private static IWebHost BuildWebHost(IConfigurationRoot configurationRoot)
        {
            // Read our custom configuration
            var jsonConfig = Configuration.Load(configurationRoot);
            var webUrl = new Uri(jsonConfig.App.TrustedOrigins[0]);

            return new WebHostBuilder()
                
                .UseConfiguration(configurationRoot)
                
                // Enable our JSON configuration object to be injected into other classes
                .ConfigureServices(services => 
                {
                    services.AddSingleton(jsonConfig);
                })
                
                // Configure logging to use our JSON configuration and to output to the console
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .AddConfiguration(configurationRoot.GetSection("Logging"))
                        .AddConsole();
                })

                // Configure the Kestrel web server to listen over SSL
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, webUrl.Port, listenOptions =>
                    {   
                        listenOptions.UseHttps($"./certs/{jsonConfig.App.SslCertificateFileName}", jsonConfig.App.SslCertificatePassword);
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
