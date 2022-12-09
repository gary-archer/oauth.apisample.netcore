namespace SampleApi.Host.Startup
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.OAuth;
    using SampleApi.Plumbing.OAuth.ClaimsCaching;
    using SampleApi.Plumbing.Security;
    using SampleApi.Plumbing.Utilities;

    /*
     * A class to manage composing core API behaviour
     */
    public sealed class BaseCompositionRoot
    {
        private OAuthConfiguration oauthConfiguration;
        private CustomClaimsProvider customClaimsProvider;
        private LoggingConfiguration loggingConfiguration;
        private LoggerFactory loggerFactory;
        private HttpProxy httpProxy;
        private IServiceCollection services;

        /*
         * Indicate that we're using OAuth and receive the configuration
         */
        public BaseCompositionRoot UseOAuth(OAuthConfiguration oauthConfiguration)
        {
            this.oauthConfiguration = oauthConfiguration;
            return this;
        }

        /*
         * Receive an object to manage processing claims
         */
        public BaseCompositionRoot WithCustomClaimsProvider(CustomClaimsProvider customClaimsProvider)
        {
            this.customClaimsProvider = customClaimsProvider;
            return this;
        }

        /*
         * Receive the logging configuration so that we can create objects related to logging and error handling
         */
        public BaseCompositionRoot WithLogging(LoggingConfiguration loggingConfiguration, ILoggerFactory loggerFactory)
        {
            this.loggingConfiguration = loggingConfiguration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public BaseCompositionRoot WithProxyConfiguration(bool enabled, string url)
        {
            this.httpProxy = new HttpProxy(enabled, url);
            return this;
        }

        /*
         * Store an ASP.Net core services reference which we will update later
         */
        public BaseCompositionRoot WithServices(IServiceCollection services)
        {
            this.services = services;
            return this;
        }

        /*
         * Prepare and register objects
         */
        public void Register()
        {
            // Register dependencies for logging and error handling
            this.RegisterBaseDependencies();

            // Register the Microsoft thread safe memory cache
            this.services.AddDistributedMemoryCache();

            // Get the container, then register dependencies for OAuth processing and claims handling
            using (var container = this.services.BuildServiceProvider())
            {
                var cache = container.GetService<IDistributedCache>();
                this.RegisterOAuthDependencies(cache);
                this.RegisterClaimsDependencies(cache, container);
            }
        }

        /*
         * Register dependencies specific to logging
         */
        private void RegisterBaseDependencies()
        {
            // The log entry is scoped to the current request and created via this factory method
            this.services.AddSingleton(this.loggingConfiguration);
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
         * Register OAuth dependencies
         */
        private void RegisterOAuthDependencies(IDistributedCache cache)
        {
            this.services.AddSingleton(this.oauthConfiguration);

            this.services.AddScoped<OAuthAuthenticator>();
            if (this.oauthConfiguration.ClaimsStrategy == "apiLookup")
            {
                this.services.AddScoped<IAuthorizer, ClaimsCachingAuthorizer>();
            }
            else
            {
                this.services.AddScoped<IAuthorizer, StandardAuthorizer>();
            }

            // JWKS keys are stored in a thread safe cache
            var jwksCache = new JwksCache(cache);
            this.services.AddSingleton(new JsonWebKeyResolver(this.oauthConfiguration, jwksCache, this.httpProxy));
        }

        /*
         * Register Claims related dependencies
         */
        private void RegisterClaimsDependencies(IDistributedCache cache, ServiceProvider container)
        {
            // Register extra objects if using claims caching
            if (this.oauthConfiguration.ClaimsStrategy == "apiLookup")
            {
                this.services.AddScoped<UserInfoClient>();

                var claimsCache = new ClaimsCache(
                    cache,
                    this.oauthConfiguration.ClaimsCache.TimeToLiveMinutes,
                    container);
                this.services.AddSingleton(claimsCache);
            }

            // Register an object to manage custom claims
            this.services.AddSingleton(this.customClaimsProvider);

            // Make the claims principal injectable
            this.services.AddScoped(ctx => ctx.GetService<IHttpContextAccessor>().HttpContext.User);
        }
    }
}
