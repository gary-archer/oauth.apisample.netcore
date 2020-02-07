namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * A builder style class to configure framework behaviour and to register its dependencies
     */
    public class FrameworkBuilder
    {
        /*
         * Add standard framework .Net Core middleware classes
         */
        public FrameworkBuilder AddMiddleware(IApplicationBuilder api)
        {
            api.UseMiddleware<LoggerMiddleware>();
            api.UseMiddleware<UnhandledExceptionMiddleware>();
            api.UseMiddleware<CustomHeaderMiddleware>();
            return this;
        }

        /*
         * Add framework dependencies to the container
         */
        public FrameworkBuilder Register(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<LogEntry>();
            return this;
        }
    }
}
