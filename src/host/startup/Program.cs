namespace FinalApi.Host.Startup
{
    using System;
    using System.Net;
    using FinalApi.Host.Configuration;
    using FinalApi.Logic.Claims;
    using FinalApi.Plumbing.Dependencies;
    using FinalApi.Plumbing.Logging;
    using FinalApi.Plumbing.Middleware;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * The entry point class
     */
    public static class Program
    {
        /*
         * The entry point method
         */
        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactoryBuilder.Create();

            try
            {
                // Create and run the API
                CreateApi(loggerFactory).Run();
            }
            catch (Exception ex)
            {
                // Report startup errors
                loggerFactory.LogStartupError(ex);
            }
        }

        /*
         * Build an application to handle OAuth requests over HTTP
         */
        private static WebApplication CreateApi(Plumbing.Logging.ILoggerFactory loggerFactory)
        {
            // Load the configuration file
            var configuration = Configuration.LoadAsync("./api.config.json").Result;

            // Create the builder
            var builder = WebApplication.CreateBuilder();

            // Configure JSON logging
            loggerFactory.Configure(builder.Logging, configuration.Logging);
            builder.Services.AddSingleton(configuration);

            // Listen over HTTPS if we configure certificate details, or HTTP otherwise
            builder.WebHost
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Api.Port, listenOptions =>
                    {
                        if (!string.IsNullOrWhiteSpace(configuration.Api.SslCertificateFileName) &&
                            !string.IsNullOrWhiteSpace(configuration.Api.SslCertificatePassword))
                        {
                            listenOptions.UseHttps(
                                configuration.Api.SslCertificateFileName,
                                configuration.Api.SslCertificatePassword);
                        }
                    });
                });

            // Do custom JWT authentication, to create a useful claims principal and to control OAuth error handling.
            string scheme = "Bearer";
            builder.Services.AddAuthentication(scheme)
                .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(scheme, null);

            // Unless AllowAnonymous is used, require successful authentication before running controller actions.
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            // Register the API's routes
            builder.Services.AddControllers();

            // Register the Microsoft thread safe memory cache
            builder.Services.AddDistributedMemoryCache();

            // Register dependencies with the container
            new CompositionRoot(builder.Services)
                .AddConfiguration(configuration)
                .AddLogging(configuration.Logging, loggerFactory)
                .AddProxyConfiguration(configuration.Api.UseProxy, configuration.Api.ProxyUrl)
                .AddExtraClaimsProvider(new ExtraClaimsProvider())
                .Register();

            // Create the API
            var api = builder.Build();

            // Configure .NET security
            api.UseRouting();
            api.UseAuthentication();
            api.UseAuthorization();

            // Configure middleware classes
            api.UseMiddleware<LoggerMiddleware>();
            api.UseMiddleware<UnhandledExceptionMiddleware>();
            api.UseMiddleware<CustomHeaderMiddleware>();

            // Use controller attributes for API request routing
            api.MapControllers();

            System.Console.WriteLine("*** HERE");
            return api;
        }
    }
}
