namespace Framework.Api.OAuth.Security
{
    using System;
    using System.Net.Http;
    using Framework.Api.Base.Utilities;
    using Framework.Api.OAuth.Claims;
    using Framework.Api.OAuth.Configuration;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /*
     * Helper methods for setting up authentication
     */
    public sealed class AuthorizationFilterBuilder<TClaims>
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
        public AuthorizationFilterBuilder(AuthorizationFilterOptions options)
        {
            this.configuration = options.OAuthConfiguration;
        }

        /*
         * Provide the type of custom claims provider
         */
        public AuthorizationFilterBuilder<TClaims> WithCustomClaimsProvider<TProvider>()
            where TProvider : CustomClaimsProvider<TClaims>
        {
            this.customClaimsProviderType = typeof(TProvider);
            return this;
        }

        /*
         * Store an ASP.Net core services reference which we will update later
         */
        public AuthorizationFilterBuilder<TClaims> WithServices(IServiceCollection services)
        {
            this.services = services;
            return this;
        }

        /*
         * Store an object to manage HTTP debugging
         */
        public AuthorizationFilterBuilder<TClaims> WithHttpDebugging(bool enabled, string url)
        {
            this.httpProxyFactory = () => new ProxyHttpHandler(enabled, url);
            return this;
        }

        /*
         * Do the work of validating the configuration and finalizing configuration
         */
        public void Build()
        {
            using (var provider = this.services.BuildServiceProvider())
            {
                // Check prerequisites and get the Microsoft cache
                this.VerifyPrerequisite<IHttpContextAccessor>(provider);
                IDistributedCache cache = this.VerifyPrerequisite<IDistributedCache>(provider);

                // Load issuer metadata during startup
                var issuerMetadata = new IssuerMetadata(this.configuration, this.httpProxyFactory);
                issuerMetadata.Load().Wait();

                // Create the thread safe claims cache
                var claimsCache = new ClaimsCache<TClaims>(cache, this.configuration, provider.GetService<ILoggerFactory>());

                // Create a default custom claims provider if needed
                if (this.customClaimsProviderType == null)
                {
                    this.customClaimsProviderType = typeof(CustomClaimsProvider<TClaims>);
                }

                // Create a default proxy object if needed
                if (this.httpProxyFactory == null)
                {
                    this.httpProxyFactory = () => new ProxyHttpHandler(false, null);
                }

                // Update dependency injection
                this.RegisterAuthenticationDependencies(issuerMetadata, claimsCache);
            }
        }

        /*
         * Verify and return a prerequisite service
         */
        private T VerifyPrerequisite<T>(ServiceProvider provider)
            where T : class
        {
            var result = provider.GetService<T>();
            if (result == null)
            {
                throw new InvalidOperationException($"The prerequisite service {typeof(T).Name} has not been configured");
            }

            return result;
        }

        /*
         * Register framework dependencies
         */
        private void RegisterAuthenticationDependencies(IssuerMetadata issuerMetadata, ClaimsCache<TClaims> cache)
        {
            // Register singletons
            this.services.AddSingleton(this.configuration);
            this.services.AddSingleton(issuerMetadata);
            this.services.AddSingleton(cache);
            this.services.AddSingleton(this.httpProxyFactory);

            // Register OAuth per request dependencies
            this.services.AddScoped<Authorizer<TClaims>>();
            this.services.AddScoped<Authenticator>();
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
