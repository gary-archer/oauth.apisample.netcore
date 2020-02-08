﻿namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Configuration;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Middleware;
    using Framework.Base.Abstractions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * A builder style class to configure framework behaviour and to register its dependencies
     */
    public sealed class FrameworkBuilder
    {
        private readonly FrameworkConfiguration configuration;
        private readonly LoggerFactory loggerFactory;
        private ApplicationExceptionHandler applicationExceptionHandler;

        public FrameworkBuilder(FrameworkConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.loggerFactory = (LoggerFactory)loggerFactory;
            this.applicationExceptionHandler = new ApplicationExceptionHandler();
        }

        /*
         * Allow the application handler to be overridden
         */
        public FrameworkBuilder WithApplicationExceptionHandler(ApplicationExceptionHandler applicationExceptionHandler)
        {
            this.applicationExceptionHandler = applicationExceptionHandler;
            return this;
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
            services.AddSingleton(this.applicationExceptionHandler);

            // The log entry is scoped to the current request and created via this factory method
            services.AddScoped<ILogEntry>(
                ctx =>
                {
                    return this.loggerFactory.CreateLogEntry();
                });
        }
    }
}
