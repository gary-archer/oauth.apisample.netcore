namespace SampleApi.Host.Startup
{
    using System;
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
            // Configure .Net Core authentication to run on all paths except particular exceptions
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
                api => api.UseAuthentication());

            // Configure .Net Core middleware classes to run on all API paths
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
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
            // Configure the API's OAuth behaviour
            this.ConfigureOAuth(services);

            // Register the API's dependencies
            this.ConfigureBaseDependencies(services);
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
         * customize OAuth handling to use a library, and set up a global authorization filter
         */
        private void ConfigureOAuth(IServiceCollection services)
        {
            string scheme = "Bearer";
            services.AddAuthentication(scheme)
                    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(scheme, null);

            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });
        }

        /*
         * Configure dependencies used for cross cutting concerns
         */
        private void ConfigureBaseDependencies(IServiceCollection services)
        {
            new BaseCompositionRoot()
                .UseOAuth(this.configuration.OAuth)
                .WithCustomClaimsProvider(new SampleCustomClaimsProvider())
                .WithLogging(this.configuration.Logging, this.loggerFactory)
                .WithProxyConfiguration(this.configuration.Api.UseProxy, this.configuration.Api.ProxyUrl)
                .WithServices(services)
                .Register();
        }

        /*
         * These could be request scoped, but I prefer transient for non HTTP related classes
         */
        private void ConfigureApiDependencies(IServiceCollection services)
        {
            services.AddTransient<JsonReader>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<CompanyService>();
        }
    }
}
