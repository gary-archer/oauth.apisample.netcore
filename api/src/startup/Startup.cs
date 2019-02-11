namespace BasicApi.Startup
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;    
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using BasicApi.Configuration;
    using BasicApi.Entities;
    using BasicApi.Logic;
    using BasicApi.Plumbing.OAuth;
    using BasicApi.Plumbing.Utilities;
    using BasicApi.Plumbing.Errors;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    /*
     * The application startup class
     */
    public class Startup
    {
        /*
         * Our injected configuration
         */
        private readonly Configuration jsonConfig;

        /*
         * Construct our application startup class from configuration
         */
        public Startup(Configuration jsonConfig)
        {
            this.jsonConfig = jsonConfig;
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

            // Set up the singleton claims cache object
            services.AddDistributedMemoryCache();
            IDistributedCache cache = null;
            using(var provider = services.BuildServiceProvider())
            {
                cache = provider.GetService<IDistributedCache>();
            }
            var claimsCache = new ClaimsCache(cache);
            
            // Load issuer metadata during startup
            Func<ProxyHttpHandler> proxyFactory = () => new ProxyHttpHandler(this.jsonConfig.App);
            var issuerMetadata = new IssuerMetadata(this.jsonConfig.OAuth, proxyFactory);
            issuerMetadata.Load().Wait();

            // Add our custom authentication handler, to manage introspection and claims caching
            var builder = services
                .AddAuthentication("Bearer")
                .AddScheme<CustomAuthenticationOptions, CustomAuthenticationHandler>("Bearer", options => {
                        options.OAuthConfiguration = this.jsonConfig.OAuth;
                        options.ProxyHandlerFactory = proxyFactory;
                        options.IssuerMetadata = issuerMetadata;
                        options.ClaimsCache = claimsCache;
                    });

            // Ensure that all API requests to controllers are verified by the above handlers
            services.AddMvc(options => 
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Finally register our API's dependencies
            this.RegisterApiDependencies(services);
        }

        /*
         * Called by the runtime and used to configure the HTTP request pipeline
         */
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure API CORS requests
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method == "OPTIONS",
                api => app.UseCors("api"));

            // Configure the API to use authentication and unhandled exception middleware
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method != "OPTIONS",
                api => {
                    api.UseAuthentication();
                    api.UseMiddleware<UnhandledExceptionMiddleware>();
                });

            // Configure behaviour of SPA requests for static content
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/spa")),
                web => this.ConfigureWebServer(web));
            
            // All requests use ASP.Net core
            app.UseMvc();
        }

        /*
         * Express how to create dependencies needed by our API controllers, and their lifetimes
         */
        private void RegisterApiDependencies(IServiceCollection services)
        {
            // Ensure that the HTTP context is available during dependency resolution
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // These simple items are created as part of controller autowiring
            services.AddScoped<AuthorizationRulesRepository>();
            services.AddScoped<JsonReader>();
            services.AddScoped<CompanyRepository>();

            // The claims middleware populates the ApiClaims object and sets it against the HTTP context's claims principal
            // When controller operations execute they access the HTTP context and extract the claims
            services.AddScoped<ApiClaims>(
                ctx => ctx.GetService<IHttpContextAccessor>().HttpContext.User.DeserializeApiClaims());
        }

        /*
         * For this sample the API also serves web static content, which would not be done by a real API
         */
        private void ConfigureWebServer(IApplicationBuilder app)
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
