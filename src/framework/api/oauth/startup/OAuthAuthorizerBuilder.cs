namespace Framework.Api.OAuth.Startup
{
    using System;
    using System.Net.Http;
    using Framework.Api.Base.Claims;
    using Framework.Api.Base.Security;
    using Framework.Api.Base.Utilities;
    using Framework.Api.OAuth.Claims;
    using Framework.Api.OAuth.Configuration;
    using Framework.Api.OAuth.Security;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /*
     * Build an authorizer filter for OAuth token validation and claims caching
     */
    public sealed class OAuthAuthorizerBuilder<TClaims>
        where TClaims : CoreApiClaims, new()
    {
        // Our OAuth configuration
        private readonly OAuthConfiguration configuration;

        // The type of custom claims provider
        private Type customClaimsProviderType;

        // The ASP.Net Core services we will configure
        private IServiceCollection services;

        // An object to support HTTP debugging
        private Func<HttpClientHandler> httpProxyFactory;

        /*
         * Create our builder and receive our options
         */
        public OAuthAuthorizerBuilder(OAuthConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /*
         * Provide the type of custom claims provider
         */
        public OAuthAuthorizerBuilder<TClaims> WithCustomClaimsProvider<TProvider>()
            where TProvider : CustomClaimsProvider<TClaims>
        {
            this.customClaimsProviderType = typeof(TProvider);
            return this;
        }

        /*
         * Store an ASP.Net core services reference which we will update later
         */
        public OAuthAuthorizerBuilder<TClaims> WithServices(IServiceCollection services)
        {
            this.services = services;
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public OAuthAuthorizerBuilder<TClaims> WithHttpDebugging(bool enabled, string url)
        {
            this.httpProxyFactory = () => new ProxyHttpHandler(enabled, url);
            return this;
        }

        /*
         * Prepare objects needed for OAuth Authorization
         */
        public void Register()
        {
            using (var container = this.services.BuildServiceProvider())
            {
                // Load issuer metadata during startup
                var issuerMetadata = new IssuerMetadata(this.configuration, this.httpProxyFactory);
                issuerMetadata.Load().Wait();

                // Create the thread safe claims cache and pass it a container reference
                this.services.AddDistributedMemoryCache();
                var cache = container.GetService<IDistributedCache>();
                var claimsCache = new ClaimsCache<TClaims>(cache, this.configuration, container);

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

                // Update dependency injection
                this.RegisterOAuthDependencies(issuerMetadata, claimsCache);
            }
        }

        /*
         * Register OAuth framework dependencies
         */
        private void RegisterOAuthDependencies(IssuerMetadata issuerMetadata, ClaimsCache<TClaims> cache)
        {
            // Register singletons
            this.services.AddSingleton(this.configuration);
            this.services.AddSingleton(issuerMetadata);
            this.services.AddSingleton(cache);
            this.services.AddSingleton(this.httpProxyFactory);

            // Register OAuth per request dependencies
            this.services.AddScoped<IAuthorizer, OAuthAuthorizer<TClaims>>();
            this.services.AddScoped<OAuthAuthenticator>();
            this.services.AddScoped(typeof(CustomClaimsProvider<TClaims>), this.customClaimsProviderType);

            // The claims middleware populates the TClaims object and sets it against the HTTP context's claims principal
            // When controller operations execute they access the HTTP context and extract the claims
            this.services.AddScoped(
                ctx =>
                {
                    var claims = new TClaims();
                    claims.Load(ctx.GetService<IHttpContextAccessor>().HttpContext.User);
                    return claims;
                });
        }
    }
}
