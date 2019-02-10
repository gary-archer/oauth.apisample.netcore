namespace BasicApi.Plumbing.Utilities
{
    using Microsoft.Extensions.Logging;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.OAuth;
    using BasicApi.Startup;
    using Microsoft.Extensions.DependencyInjection;

    /*
     * Encapsulate logging logic here
     */
    public static class LoggingHelper
    {
        /*
         * Create a logger for reporting startup exceptions
         */
        public static ILogger CreateStartupLogger()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            using(var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                using(var loggerFactory = serviceProvider.GetService<ILoggerFactory>())
                {
                    return loggerFactory.CreateLogger<Startup>();
                }
            }
        }

        /*
         * Reduce output to only sources we care about
         */
        public static bool Filter(string category, LogLevel level)
        {
            switch (category)
            {
                case var a when a == typeof(Startup).FullName:
                case var b when b == typeof(ClaimsMiddleware).FullName:
                case var c when c == typeof(AuthenticationMiddlewareWithErrorHandling).FullName:
                case var d when d == typeof(UnhandledExceptionMiddleware).FullName:
                    return true;

                default:
                    return false;
            }
        }

        
    }
}