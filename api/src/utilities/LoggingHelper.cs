namespace BasicApi.Utilities
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using BasicApi.Startup;

    /// <summary>
    /// Logging utility methods
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// Create a logger for reporting startup exceptions
        /// </summary>
        /// <returns>The logger</returns>
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