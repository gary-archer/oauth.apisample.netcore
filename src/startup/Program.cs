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

        /*
         * Build a host to handle HTTP requests
         */
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
