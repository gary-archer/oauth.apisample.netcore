namespace FinalApi.Plumbing.Dependencies
{
    using FinalApi.Host.Configuration;
    using FinalApi.Logic.Repositories;
    using FinalApi.Logic.Services;
    using FinalApi.Logic.Utilities;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Logging;
    using FinalApi.Plumbing.OAuth;
    using FinalApi.Plumbing.Utilities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * A class to manage dependency injection composition at application startup
     */
    public sealed class CompositionRoot
    {
        private readonly IServiceCollection services;
        private Configuration configuration;
        private IExtraClaimsProvider extraClaimsProvider;
        private LoggingConfiguration loggingConfiguration;
        private LoggerFactory loggerFactory;
        private HttpProxy httpProxy;

        /*
         * Receive the DI container
         */
        public CompositionRoot(IServiceCollection services)
        {
            this.services = services;
        }

        /*
         * Receive configuration
         */
        public CompositionRoot AddConfiguration(Configuration configuration)
        {
            this.configuration = configuration;
            return this;
        }

        /*
         * Receive an object that customizes the claims principal
         */
        public CompositionRoot AddExtraClaimsProvider(IExtraClaimsProvider extraClaimsProvider)
        {
            this.extraClaimsProvider = extraClaimsProvider;
            return this;
        }

        /*
         * Receive the logging configuration
         */
        public CompositionRoot AddLogging(LoggingConfiguration loggingConfiguration, ILoggerFactory loggerFactory)
        {
            this.loggingConfiguration = loggingConfiguration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public CompositionRoot AddProxyConfiguration(bool enabled, string url)
        {
            this.httpProxy = new HttpProxy(enabled, url);
            return this;
        }

        /*
         * Prepare and register objects
         */
        public void Register()
        {
            // Register dependencies for logging and error handling
            this.RegisterBaseDependencies();

            // Get the container, then register dependencies for OAuth processing and claims handling
            using (var container = this.services.BuildServiceProvider())
            {
                var cache = container.GetService<IDistributedCache>();
                this.RegisterOAuthDependencies(cache);
                this.RegisterClaimsDependencies(cache, container);
            }

            // Register objects needed by application logic
            this.RegisterApplicationDependencies();
        }

        /*
         * Register base dependencies for logging and using an HTTP proxy
         */
        private void RegisterBaseDependencies()
        {
            // The log entry is scoped to the current request and created via this factory method
            this.services.AddSingleton(this.loggingConfiguration);
            this.services.AddSingleton(this.loggerFactory);

            this.services.AddScoped<ILogEntry>(
                ctx =>
                {
                    return this.loggerFactory.CreateLogEntry();
                });

            // Register HTTP related objects
            this.services.AddSingleton(this.httpProxy);
            this.services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /*
         * Register dependencies used for OAuth processing
         */
        private void RegisterOAuthDependencies(IDistributedCache cache)
        {
            // Register the configuration
            this.services.AddSingleton(this.configuration.OAuth);

            // Create the main objects for validating tokens and creating the claims principal
            this.services.AddScoped<AccessTokenValidator>();
            this.services.AddScoped<OAuthFilter>();

            // Register a singleton to store JWKS keys in a thread safe cache
            var jwksCache = new JwksCache(cache);
            this.services.AddSingleton(new JsonWebKeyResolver(this.configuration.OAuth, jwksCache, this.httpProxy));
        }

        /*
         * Register claims related dependencies
         */
        private void RegisterClaimsDependencies(IDistributedCache cache, ServiceProvider container)
        {
            // Register an object to provide extra authorization values
            this.services.AddSingleton(this.extraClaimsProvider);

            // Register a cache for extra values from the API's own data
            var claimsCache = new ClaimsCache(
                cache,
                this.configuration.OAuth.ClaimsCacheTimeToLiveMinutes,
                container);
            this.services.AddSingleton(claimsCache);

            // Make the claims principal injectable
            this.services.AddScoped(ctx =>
                ctx.GetService<IHttpContextAccessor>().HttpContext.User as CustomClaimsPrincipal);
        }

        /*
         * Register objects used by application logic
         */
        private void RegisterApplicationDependencies()
        {
            this.services.AddTransient<CompanyService>();
            this.services.AddTransient<CompanyRepository>();
            this.services.AddTransient<UserRepository>();
            this.services.AddTransient<JsonReader>();
        }
    }
}
