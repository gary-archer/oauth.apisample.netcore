namespace SampleApi.Host.Startup
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.OAuth;
    using SampleApi.Plumbing.Security;
    using SampleApi.Plumbing.Utilities;

    /*
     * A class to manage composing core API behaviour
     */
    public sealed class BaseCompositionRoot
    {
        private LoggingConfiguration loggingConfiguration;
        private LoggerFactory loggerFactory;
        private OAuthConfiguration oauthConfiguration;
        private ClaimsConfiguration claimsConfiguration;
        private Type customClaimsProviderType;
        private Func<HttpClientHandler> httpProxyFactory;
        private IServiceCollection services;

        /*
         * Receive the logging configuration so that we can create objects related to logging and error handling
         */
        public BaseCompositionRoot UseDiagnostics(LoggingConfiguration loggingConfiguration, ILoggerFactory loggerFactory)
        {
            this.loggingConfiguration = loggingConfiguration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
            return this;
        }

        /*
         * Indicate that we're using OAuth and receive the configuration
         */
        public BaseCompositionRoot UseOAuth(OAuthConfiguration oauthConfiguration)
        {
            this.oauthConfiguration = oauthConfiguration;
            return this;
        }

        /*
         * Receive information used for claims caching
         */
        public BaseCompositionRoot UseClaimsCaching(ClaimsConfiguration claimsConfiguration)
        {
            this.claimsConfiguration = claimsConfiguration;
            return this;
        }

        /*
         * Provide the type of custom claims provider
         */
        public BaseCompositionRoot WithCustomClaimsProvider()
        {
            this.customClaimsProviderType = typeof(TProvider);
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public BaseCompositionRoot WithHttpDebugging(bool enabled, string url)
        {
            this.httpProxyFactory = () => new ProxyHttpHandler(enabled, url);
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
            using (var container = this.services.BuildServiceProvider())
            {
                // Register dependencies for logging and error handling
                this.RegisterBaseDependencies();

                // Register dependencies for OAuth processing
                if (this.oauthConfiguration != null)
                {
                    this.RegisterOAuthDependencies();
                }

                // Register claims related dependencies
                this.RegisterClaimsDependencies(container);

                // Create an injectable object for managing proxy connections
                if (this.httpProxyFactory == null)
                {
                    this.httpProxyFactory = () => new ProxyHttpHandler(false, null);
                }
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

            // Register other objects
            this.services.AddSingleton(this.httpProxyFactory);
            this.services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /*
         * Register OAuth dependencies
         */
        private void RegisterOAuthDependencies()
        {
            // Load issuer metadata into an object
            var issuerMetadata = new IssuerMetadata(this.oauthConfiguration, this.httpProxyFactory);
            issuerMetadata.Load().Wait();

            // Register singletons
            this.services.AddSingleton(this.oauthConfiguration);
            this.services.AddSingleton(issuerMetadata);

            // Register OAuth per request dependencies
            this.services.AddScoped<IAuthorizer, OAuthAuthorizer>();
            this.services.AddScoped<OAuthAuthenticator>();
        }

        /*
         * Register Claims related dependencies
         */
        private void RegisterClaimsDependencies(ServiceProvider container)
        {
            // Create the thread safe claims cache and pass it a container reference
            this.services.AddDistributedMemoryCache();
            var cache = container.GetService<IDistributedCache>();
            var claimsCache = new ClaimsCache(cache, this.claimsConfiguration, container);

            // Create a default injecteable custom claims provider if needed
            if (this.customClaimsProviderType == null)
            {
                this.customClaimsProviderType = typeof(CustomClaimsProvider);
            }

            // Register singletons
            this.services.AddSingleton(claimsCache);

            // Register OAuth per request dependencies
            this.services.AddScoped(typeof(CustomClaimsProvider), this.customClaimsProviderType);

            // Claims are injected into this holder at runtime
            this.services.AddScoped<ClaimsHolder>();

            // The underlying claims object can then be retrieved via this factory method
            this.services.AddScoped<ApiClaims>(ctx =>
            {
                return ctx.GetService<ClaimsHolder>().Value;
            });
        }
    }
}
