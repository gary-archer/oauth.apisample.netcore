namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Claims;
    using Framework.Api.Base.Security;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * Build a simple authorizer for receiving claims via headers
     */
    public sealed class HeaderAuthorizerBuilder
    {
        // The ASP.Net Core services we will configure
        private IServiceCollection services;

        /*
         * Store an ASP.Net core services reference which we will update later
         */
        public HeaderAuthorizerBuilder WithServices(IServiceCollection services)
        {
            this.services = services;
            return this;
        }

        /*
         * Prepare objects needed for OAuth Authorization
         */
        public void Register()
        {
            // Add singleton dependencies
            this.services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register OAuth per request dependencies
            this.services.AddScoped<IAuthorizer, HeaderAuthorizer>();
            this.services.AddScoped<HeaderAuthenticator>();

            // Claims are injected into this holder at runtime
            this.services.AddScoped<ClaimsHolder>();

            // The underlying claims object can then be retrieved via this factory method
            this.services.AddScoped<CoreApiClaims>(ctx =>
            {
                return ctx.GetService<ClaimsHolder>().Value;
            });
        }
    }
}
