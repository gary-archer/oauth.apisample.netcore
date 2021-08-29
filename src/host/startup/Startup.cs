namespace SampleApi.Host.Startup
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Claims;
    using SampleApi.Host.Configuration;
    using SampleApi.Logic.Repositories;
    using SampleApi.Logic.Utilities;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Middleware;
    using SampleApi.Plumbing.Security;

    /*
     * The application startup class
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
            // Configure CORS for API pre flight requests
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method == "OPTIONS",
                api => app.UseCors("api"));

            // Configure .Net Core authentication to run on all paths except those marked with [AllowAnonymous]
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) &&
                       !ctx.Request.Path.StartsWithSegments(new PathString("/api/customclaims")) &&
                       ctx.Request.Method != "OPTIONS",
                api => api.UseAuthentication());

            // Configure .Net Core middleware
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) &&
                       ctx.Request.Method != "OPTIONS",
                api => this.ConfigureApiMiddleware(api));

            // Use controller attributes for API request routing
            app.UseRouting();
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
            // Support CORS for our trusted origins
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "api",
                    policy => policy.WithOrigins(this.configuration.Api.WebTrustedOrigins.ToArray())
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            // Register our API's dependencies
            this.ConfigureApiDependencies(services);
        }

        /*
         * Set up the API with cross cutting concerns
         */
        private void ConfigureApiMiddleware(IApplicationBuilder api)
        {
            api.UseMiddleware<LoggerMiddleware>();
            api.UseMiddleware<UnhandledExceptionMiddleware>();
            api.UseMiddleware<CustomHeaderMiddleware>();
        }

        /*
         * Set up API dependencies
         */
        private void ConfigureApiDependencies(IServiceCollection services)
        {
            // Configure common code
            this.ConfigureBaseDependencies(services);

            // Register dependencies specific to this service
            services.AddScoped<JsonReader>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<CompanyService>();
        }

        /*
         * Configure dependencies used for cross cutting concerns
         */
        private void ConfigureBaseDependencies(IServiceCollection services)
        {
            // Customise the whole of the OAuth processing
            string scheme = "Bearer";
            services.AddAuthentication(scheme)
                    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(scheme, null);

            // Add a global filter to ensure that all API requests are authorized
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Register dependencies used to implement cross cutting concerns
            new BaseCompositionRoot()
                .UseOAuth(this.configuration.OAuth)
                .WithClaimsProvider(new SampleClaimsProvider())
                .WithLogging(this.configuration.Logging, this.loggerFactory)
                .WithProxyConfiguration(this.configuration.Api.UseProxy, this.configuration.Api.ProxyUrl)
                .WithServices(services)
                .Register();
        }
    }
}
