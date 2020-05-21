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
    using SampleApi.Plumbing.Utilities;

    /*
     * A class to manage composing core API behaviour
     */
    public sealed class CompositionRoot<TClaims>
        where TClaims : CoreApiClaims, new()
    {
        // Injected items
        private readonly LoggingConfiguration loggingConfiguration;
        private readonly OAuthConfiguration oauthConfiguration;
        private readonly LoggerFactory loggerFactory;

        // The ASP.Net Core services we will configure
        private IServiceCollection services;

        // An object to support HTTP debugging
        private Func<HttpClientHandler> httpProxyFactory;

        // The type of custom claims provider
        private Type customClaimsProviderType;

        /*
         * Create our builder and receive our options
         */
        public CompositionRoot(
            LoggingConfiguration loggingConfiguration,
            OAuthConfiguration oauthConfiguration,
            ILoggerFactory loggerFactory)
        {
            this.loggingConfiguration = loggingConfiguration;
            this.oauthConfiguration = oauthConfiguration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
        }

        /*
         * Store an ASP.Net core services reference which we will update later
         */
        public CompositionRoot<TClaims> WithServices(IServiceCollection services)
        {
            this.services = services;
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public CompositionRoot<TClaims> WithHttpDebugging(bool enabled, string url)
        {
            this.httpProxyFactory = () => new ProxyHttpHandler(enabled, url);
            return this;
        }

        /*
         * Provide the type of custom claims provider
         */
        public CompositionRoot<TClaims> WithCustomClaimsProvider<TProvider>()
            where TProvider : CustomClaimsProvider<TClaims>
        {
            this.customClaimsProviderType = typeof(TProvider);
            return this;
        }

        /*
         * Prepare and register objects
         */
        public void Register()
        {
            using (var container = this.services.BuildServiceProvider())
            {
                // Load issuer metadata during startup
                var issuerMetadata = new IssuerMetadata(this.oauthConfiguration, this.httpProxyFactory);
                issuerMetadata.Load().Wait();

                // Create the thread safe claims cache and pass it a container reference
                this.services.AddDistributedMemoryCache();
                var cache = container.GetService<IDistributedCache>();
                var claimsCache = new ClaimsCache<TClaims>(cache, this.oauthConfiguration, container);

                // Create a default injecteable custom claims provider if needed
                if (this.customClaimsProviderType == null)
                {
                    this.customClaimsProviderType = typeof(CustomClaimsProvider<TClaims>);
                }

                // Create a default injecteable proxy object if needed
                if (this.httpProxyFactory == null)
                {
                    this.httpProxyFactory = () => new ProxyHttpHandler(false, null);
                }

                // Update the container
                this.RegisterBaseDependencies();
                this.RegisterOAuthDependencies(issuerMetadata, claimsCache);
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
        private void RegisterOAuthDependencies(IssuerMetadata issuerMetadata, ClaimsCache<TClaims> cache)
        {
            // Register singletons
            this.services.AddSingleton(this.oauthConfiguration);
            this.services.AddSingleton(issuerMetadata);
            this.services.AddSingleton(cache);

            // Register OAuth per request dependencies
            this.services.AddScoped<IAuthorizer, OAuthAuthorizer<TClaims>>();
            this.services.AddScoped<OAuthAuthenticator>();
            this.services.AddScoped(typeof(CustomClaimsProvider<TClaims>), this.customClaimsProviderType);

            // Claims are injected into this holder at runtime
            this.services.AddScoped<ClaimsHolder>();

            // The underlying claims object can then be retrieved via this factory method
            this.services.AddScoped<TClaims>(ctx =>
            {
                return (TClaims)ctx.GetService<ClaimsHolder>().Value;
            });
        }
    }
}
