namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Configuration;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Middleware;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * A builder style class to configure framework behaviour and to register its dependencies
     */
    public class FrameworkBuilder
    {
        private readonly FrameworkConfiguration configuration;

        public FrameworkBuilder(FrameworkConfiguration configuration)
        {
            this.configuration = configuration;
        }

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
        public void Register(IServiceCollection services)
        {
            // Register singletons
            services.AddSingleton(this.configuration);

            // The log entry is scoped to the current request and created via this factory method
            services.AddScoped(
                ctx =>
                {
                    return new LogEntry(this.configuration.ApiName);
                });
        }
    }
}
