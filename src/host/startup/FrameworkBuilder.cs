namespace SampleApi.Host.Startup
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using SampleApi.Host.Plumbing.Configuration;
    using SampleApi.Host.Plumbing.Logging;
    using SampleApi.Host.Plumbing.Middleware;

    /*
     * A builder style class to configure common behaviour and to register dependencies
     */
    public sealed class FrameworkBuilder
    {
        private readonly LoggingConfiguration configuration;
        private readonly LoggerFactory loggerFactory;

        public FrameworkBuilder(LoggingConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
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
            services.AddScoped<ILogEntry>(
                ctx =>
                {
                    return this.loggerFactory.CreateLogEntry();
                });
        }
    }
}
