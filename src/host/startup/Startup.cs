namespace SampleApi.Host.Startup
{
    using Framework.Api.Base.Startup;
    using Framework.Api.OAuth.Security;
    using Framework.Api.OAuth.Startup;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Authorization;
    using SampleApi.Host.Configuration;
    using SampleApi.Host.Utilities;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Repositories;
    using SampleApi.Logic.Utilities;

    /*
     * The application startup class
     */
    public class Startup
    {
        private readonly Configuration jsonConfig;

        public Startup(Configuration jsonConfig)
        {
            this.jsonConfig = jsonConfig;
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
                       ctx.Request.Path.StartsWithSegments(new PathString("/desktop")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/mobile")),
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
                    policy => policy.WithOrigins(this.jsonConfig.App.TrustedOrigins.ToArray())
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            // Make base framework dependencies injectable
            this.ConfigureApiBaseFrameworkDependencies(services);

            // Make OAuth framework dependencies injectable
            this.ConfigureApiOAuthFrameworkDependencies(services);

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

            // Add standard framework middleware classes
            new FrameworkBuilder().AddMiddleware(api);
        }

        /*
         * Add standard base API framework dependencies
         */
        private void ConfigureApiBaseFrameworkDependencies(IServiceCollection services)
        {
            new FrameworkBuilder().Register(services);
        }

        /*
         * Set up the API with cross cutting concerns
         */
        private void ConfigureApiOAuthFrameworkDependencies(IServiceCollection services)
        {
            // Indicate the type of the .Net Core custom authentication handler
            string scheme = "Bearer";
            services.AddAuthentication(scheme)
                    .AddScheme<CustomAuthenticationFilterOptions, CustomAuthenticationFilter<SampleApiClaims>>(scheme, (o) => { });

            // Indicate that all API requests are authorized, by applying the standard .Net Core filter
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Build resources that will be injected into the Custom Authentication Filter
            new OAuthAuthorizerBuilder<SampleApiClaims>(this.jsonConfig.OAuth)
                .WithCustomClaimsProvider<SampleApiClaimsProvider>()
                .WithServices(services)
                .WithHttpDebugging(this.jsonConfig.App.UseProxy, this.jsonConfig.App.ProxyUrl)
                .Register();
        }

        /*
         * Add the API's own business logic dependencies with a per request scope
         */
        private void ConfigureApiDependencies(IServiceCollection services)
        {
            services.AddScoped<JsonReader>();
            services.AddScoped<CompanyRepository>();
            services.AddScoped<CompanyService>();
        }
    }
}
