namespace SampleApi.Host.Plumbing.Logging
{
    using System;
    using Microsoft.Extensions.Logging;
    using SampleApi.Host.Plumbing.Configuration;

    /*
     * A logger factory interface
     */
    public interface ILoggerFactory
    {
        // The entry point for configuring logging
        void Configure(ILoggingBuilder builder, LoggingConfiguration configuration);

        // Handle errors that prevent startup
        void LogStartupError(Exception exception);
    }
}