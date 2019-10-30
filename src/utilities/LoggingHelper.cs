namespace BasicApi.Utilities
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using BasicApi.Startup;

    /*
     * Logging utility methods
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
    }
}