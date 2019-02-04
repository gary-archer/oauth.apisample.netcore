namespace BasicApi.Startup
{
    using System;
    using System.IO;
    using IdentityModel.AspNetCore.OAuth2Introspection;
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
    using BasicApi.Plumbing.Utilities;

    /*
     * The application startup class
     */
    public class Startup
    {
        /*
         * Our injected configuration
         */
        private IConfiguration configuration;

        /*
         * Construct our application startup class from configuration
         */
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /*
         * Called by the runtime to add services to the IOC container
         */
        public void ConfigureServices(IServiceCollection services)
        {
            // Read our custom configuration
            var appConfig = ApplicationConfiguration.Load(configuration);
            var oauthConfig = OAuthConfiguration.Load(configuration);

            // Support CORS for our trusted origins
            services.AddCors(options => 
            {
                options.AddPolicy(
                    "api", 
                    policy => policy.WithOrigins(appConfig.TrustedOrigins.ToArray())
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });
            
            // Use a memory cache for claims caching
            services.AddDistributedMemoryCache();
            
            // We use Identity Model authentication to introspect and cache tokens
            services
                .AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options =>
                {
                    // Basic OAuth settings
                    options.Authority = oauthConfig.Authority;
                    options.ClientId = oauthConfig.ClientId;
                    options.ClientSecret = oauthConfig.ClientSecret;
                    
                    // We are using introspection for JWTs so we need to override the default
                    options.SkipTokensWithDots = false;
                    
                    // Our Okta developer setup requires this but we wouldn't use it for a production solution
                    options.DiscoveryPolicy.ValidateEndpoints = false;

                    // Turn on token caching and override the default 5 minute duration
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(oauthConfig.DefaultTokenCacheMinutes);
                    
                    // Use the Okta immutable user id as the subject claim so that it is in the claims identity name
                    options.NameClaimType = "uid";

                    // Support viewing the introspection traffic in a debugger such as Fiddler or Charles
                    options.DiscoveryHttpHandler = new ProxyHttpHandler(appConfig);
                    options.IntrospectionHttpHandler = new ProxyHttpHandler(appConfig);
                });

            // Ensure that all API requests to controllers are verified by the above introspection
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

            // Configure behaviour of API options requests
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")) && ctx.Request.Method != "OPTIONS",
                api => ConfigureApi(api));

            // Configure behaviour of SPA requests for static content
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/spa")),
                web => this.ConfigureWebServer(web));
            
            // All requests use MVC
            app.UseMvc();
        }

        /*
         * Configure our API's security and other aspects
         */
        private void ConfigureApi(IApplicationBuilder app)
        {
            app.UseAuthenticationMiddlewareWithErrorHandling();
            app.UseClaimsMiddleware();
            app.UseCustomExceptionHandler();
        }

        /*
         * Express how to create dependencies needed by our API controllers, and their lifetimes
         */
        private void RegisterApiDependencies(IServiceCollection services)
        {
            // Ensure that the HTTP context is available during dependency resolution
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // These simple items are created as part of controller autowiring
            services.AddTransient<AuthorizationMicroservice>();
            services.AddTransient<JsonReader>();
            services.AddTransient<CompanyRepository>();

            // The claims middleware populates the ApiClaims object and sets it against the HTTP context's claims principal
            // When controller operations execute they access the HTTP context and extract the claims
            services.AddTransient<ApiClaims>(ctx => ctx.GetService<IHttpContextAccessor>().HttpContext.User.GetApiClaims());
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
