namespace SampleApi.Host.Startup
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
    using SampleApi.Host.Authorization;
    using SampleApi.Host.Configuration;
    using SampleApi.Host.Errors;
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
                    OAuthConfiguration = this.jsonConfig.OAuth
                })
                .WithCustomClaimsProvider<SampleApiClaimsProvider>()
                .WithServices(services)
                .WithHttpDebugging(this.jsonConfig.App.useProxy, this.jsonConfig.App.ProxyUrl)
                .Build();

            // Apply the above authorization filter to all API requests
            services.AddMvc(options => 
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });

            // Register dependencies for our API's logic, to be created per request
            services.AddScoped<JsonReader>();
            services.AddScoped<CompanyRepository>();
            services.AddScoped<CompanyService>();
        }

        /*
         * For this sample the API also serves web static content, which would not be done by a real API
         * It is expected that the SPA code sample has been built to a parallel folder
         */
        private void ConfigureWebStaticContentHosting(IApplicationBuilder app)
        {
            const string webStaticContentRoot = "../authguidance.websample.final/spa";

            // Handle custom file resolution when serving static files
            app.UseMiddleware<WebStaticContentFileResolver>();

            // This will serve index.html as the default document
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), webStaticContentRoot)),
                RequestPath = "/spa"
            });

            // This will serve JS, image and CSS files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), webStaticContentRoot)),
                RequestPath = "/spa"
            });
        }
    }
}
