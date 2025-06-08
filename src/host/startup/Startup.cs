namespace FinalApi.Host.Startup
{
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
     * The application startup class configures authentication and authorization
     */
    public class Startup
    {
        private readonly Configuration configuration;
        private readonly ILoggerFactory loggerFactory;

        /*
         * Store references to injected dependencies
         */
        public Startup(Configuration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
        }

        /*
         * Called by the runtime and used to configure the HTTP request pipeline
         */
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // Indicate that .NET security will be used
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure other middleware classes
            this.ConfigureApiMiddleware(app);

            // Use controller attributes for API request routing
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /*
         * Called by the runtime to add services to the IOC container
         */
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure the API's OAuth behaviour
            this.ConfigureOAuth(services);

            // Register the API's routes
            services.AddControllers();

            // Register the Microsoft thread safe memory cache
            services.AddDistributedMemoryCache();

            // Register dependencies with the container
            new CompositionRoot(services)
                .AddConfiguration(this.configuration)
                .AddLogging(this.configuration.Logging, this.loggerFactory)
                .AddProxyConfiguration(this.configuration.Api.UseProxy, this.configuration.Api.ProxyUrl)
                .AddExtraClaimsProvider(new ExtraClaimsProvider())
                .Register();
        }

        /*
         * Set up the API with cross cutting concerns, including JSON settings
         */
        private void ConfigureApiMiddleware(IApplicationBuilder api)
        {
            api.UseMiddleware<LoggerMiddleware>();
            api.UseMiddleware<UnhandledExceptionMiddleware>();
            api.UseMiddleware<CustomHeaderMiddleware>();
        }

        /*
         * Say how authentication and authorization will be done
         */
        private void ConfigureOAuth(IServiceCollection services)
        {
            // Do custom JWT authentication, to create a useful claims principal and to control OAuth error handling
            string scheme = "Bearer";
            services.AddAuthentication(scheme)
                    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(scheme, null);

            // Unless AllowAnonymous is used, require successful authentication before running controller actions
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });
        }
    }
}
