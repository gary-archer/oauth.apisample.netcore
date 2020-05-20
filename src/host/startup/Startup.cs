namespace SampleApi.Host.Startup
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Authorization;
    using SampleApi.Host.Claims;
    using SampleApi.Host.Configuration;
    using SampleApi.Host.Plumbing.Logging;
    using SampleApi.Host.Plumbing.OAuth;
    using SampleApi.Host.Utilities;
    using SampleApi.Logic.Repositories;
    using SampleApi.Logic.Utilities;

    /*
     * The application startup class
     */
    public class Startup
    {
        private readonly Configuration jsonConfig;
        private readonly FrameworkBuilder frameworkBuilder;

        public Startup(Configuration jsonConfig, ILoggerFactory loggerFactory)
        {
            this.jsonConfig = jsonConfig;
            this.frameworkBuilder = new FrameworkBuilder(this.jsonConfig.Logging, loggerFactory);
        }

        /*
         * Called by the runtime and used to configure the HTTP request pipeline
         */
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure API pre flight requests
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method == "OPTIONS",
                api => app.UseCors("api"));

            // Configure .Net Core middleware for the API
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method != "OPTIONS",
                api => this.ConfigureApiMiddleware(api));

            // For demo purposes our API also serves web static content for requests for the below paths
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/spa")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/loopback")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/desktop")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/android")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/ios")),
                web => WebStaticContent.Configure(web));

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
                    policy => policy.WithOrigins(this.jsonConfig.Api.TrustedOrigins.ToArray())
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            // Make base dependencies injectable
            this.ConfigureApiBaseDependencies(services);

            // Make OAuth dependencies injectable
            this.ConfigureApiOAuthDependencies(services);

            // Register our API's own dependencies
            this.ConfigureApiDependencies(services);
        }

        /*
         * Set up the API with cross cutting concerns
         */
        private void ConfigureApiMiddleware(IApplicationBuilder api)
        {
            // Ensure that authentication middleware is called for API requests
            api.UseAuthentication();

            // Add standard middleware classes
            this.frameworkBuilder.AddMiddleware(api);
        }

        /*
         * Add standard base API framework dependencies
         */
        private void ConfigureApiBaseDependencies(IServiceCollection services)
        {
            this.frameworkBuilder
                .Register(services);
        }

        /*
         * Set up the API with cross cutting concerns
         */
        private void ConfigureApiOAuthDependencies(IServiceCollection services)
        {
            // Indicate the type of our .Net Core custom authentication handler
            string scheme = "Bearer";
            services.AddAuthentication(scheme)
                    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(scheme, null);

            // Indicate that all API requests are authorized, by applying the standard .Net Core Authorize Filter
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Prepare resources that will be injected into the above custom authentication handler
            new OAuthAuthorizerBuilder<SampleApiClaims>(this.jsonConfig.OAuth)
                .WithCustomClaimsProvider<SampleApiClaimsProvider>()
                .WithServices(services)
                .WithHttpDebugging(this.jsonConfig.Api.UseProxy, this.jsonConfig.Api.ProxyUrl)
                .Register();
        }

        /*
         * Add the API's own business logic dependencies with a per request scope
         */
        private void ConfigureApiDependencies(IServiceCollection services)
        {
            services.AddScoped<JsonReader>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<CompanyService>();
        }
    }
}
