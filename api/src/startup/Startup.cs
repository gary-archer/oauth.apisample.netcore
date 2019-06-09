namespace BasicApi.Startup
{
    using System.IO;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;    
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Framework.OAuth;
    using Framework.Utilities;
    using BasicApi.Configuration;
    using BasicApi.Logic.Entities;
    using BasicApi.Errors;
    using BasicApi.Logic.Authorization;
    using BasicApi.Logic.Repositories;
    using BasicApi.Utilities;

    /// <summary>
    /// The application startup class
    /// </summary>
    public class Startup
    {
        // Our injected configuration
        private readonly Configuration jsonConfig;

        /// <summary>
        /// Construct our application startup class from configuration
        /// </summary>
        /// <param name="jsonConfig">The configuration from our JSON file</param>
        public Startup(Configuration jsonConfig)
        {
            this.jsonConfig = jsonConfig;
        }

        /// <summary>
        /// Called by the runtime and used to configure the HTTP request pipeline
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure API pre flight requests
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method == "OPTIONS",
                api => app.UseCors("api"));

            // Configure API requests to use our framework security and error handling
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method != "OPTIONS",
                api => {
                    api.UseAuthentication();
                    api.UseMiddleware<UnhandledExceptionHandler>();
                });

            // Configure behaviour of SPA requests for static content
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/spa")),
                web => this.ConfigureWebStaticContentHosting(web));

            // All requests use ASP.Net core
            app.UseMvc();
        }

        /// <summary>
        /// Called by the runtime to add services to the IOC container
        /// </summary>
        /// <param name="services">The .Net core services</param>
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
                .AddCustomAuthorizationFilter<BasicApiClaims>(new AuthorizationFilterOptions()
                {
                    OAuthConfiguration = this.jsonConfig.OAuth
                })
                .WithCustomClaimsProvider<BasicApiClaimsProvider>()
                .WithServices(services)
                .WithHttpDebugging(this.jsonConfig.App.useProxy, this.jsonConfig.App.ProxyUrl)
                .Build();

            // Indicate which API requests use our custom authentication handler
            services.AddMvc(options => 
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Register dependencies for our API's logic, to be created per request
            services.AddScoped<JsonReader>();
            services.AddScoped<CompanyRepository>();
        }

        /// <summary>
        /// For this sample the API also serves web static content, which would not be done by a real API
        /// </summary>
        /// <param name="app">The application builder</param>
        private void ConfigureWebStaticContentHosting(IApplicationBuilder app)
        {
            // This will serve index.html as the default document
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "../spa")),
                RequestPath = "/spa"
            });

            // This will serve JS, image and CSS files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "../spa")),
                RequestPath = "/spa"
            });
        }
    }
}
