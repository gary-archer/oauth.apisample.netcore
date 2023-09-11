namespace SampleApi.Host.Startup
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Configuration;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Repositories;
    using SampleApi.Logic.Services;
    using SampleApi.Logic.Utilities;
    using SampleApi.Plumbing.Dependencies;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Middleware;

    /*
     * The application startup class configures authentication and authorization
     * https://github.com/mihirdilip/aspnetcore-allowanonymous-test/blob/main/Startup.cs
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

            // Register the API's dependencies
            services.AddControllers();
            this.ConfigureBaseDependencies(services);
            this.ConfigureApiDependencies(services);
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

        /*
         * Configure dependencies used for cross cutting concerns
         */
        private void ConfigureBaseDependencies(IServiceCollection services)
        {
            new BaseCompositionRoot()
                .UseOAuth(this.configuration.OAuth)
                .WithExtraClaimsProvider(new SampleExtraClaimsProvider())
                .WithLogging(this.configuration.Logging, this.loggerFactory)
                .WithProxyConfiguration(this.configuration.Api.UseProxy, this.configuration.Api.ProxyUrl)
                .WithServices(services)
                .Register();
        }

        /*
         * Use transient scopes for non HTTP related classes
         */
        private void ConfigureApiDependencies(IServiceCollection services)
        {
            services.AddTransient<CompanyService>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<UserRepository>();
            services.AddTransient<JsonReader>();
        }
    }
}
