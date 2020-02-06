namespace SampleApi.Host.Startup
{
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Middleware;
    using Framework.Api.Base.Utilities;
    using Framework.Api.OAuth.Security;
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

            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method != "OPTIONS",
                api =>
                {
                    // Ensure that authentication middleware is called for API requests
                    api.UseAuthentication();

                    // Add framework middleware for API cross cutting concerns
                    api.UseMiddleware<LoggerMiddleware>();
                    api.UseMiddleware<UnhandledExceptionMiddleware>();
                });

            // For demo purposes we also serve static content for requests for the below paths
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/spa")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/desktop")) ||
                       ctx.Request.Path.StartsWithSegments(new PathString("/mobile")),
                web => WebStaticContent.Configure(web));

            // Use controller attributes for routing
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

            // These are prerequisites for our authentication solution
            services.AddDistributedMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add our custom authentication handler, to manage introspection and claims caching
            services
                .AddAuthentication("Bearer")
                .AddCustomAuthorizationFilter<SampleApiClaims>(new AuthorizationFilterOptions()
                {
                    OAuthConfiguration = this.jsonConfig.OAuth,
                })
                .WithCustomClaimsProvider<SampleApiClaimsProvider>()
                .WithServices(services)
                .WithHttpDebugging(this.jsonConfig.App.UseProxy, this.jsonConfig.App.ProxyUrl)
                .Build();

            // Apply the above authorization filter to all API requests
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Register our API's dependencies, which are per request scoped
            services.AddScoped<JsonReader>();
            services.AddScoped<CompanyRepository>();
            services.AddScoped<CompanyService>();
            services.AddScoped<LogEntry>();
        }
    }
}
