namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Security;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * Build a simple authorizer for receiving claims via headers
     */
    public class HeaderAuthorizerBuilder
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
            // Register OAuth per request dependencies
            this.services.AddScoped<IAuthorizer, HeaderAuthorizer>();
            this.services.AddScoped<HeaderAuthenticator>();
        }
    }
}
