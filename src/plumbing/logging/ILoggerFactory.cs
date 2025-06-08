namespace FinalApi.Plumbing.Logging
{
    using System;
    using FinalApi.Plumbing.Configuration;
    using Microsoft.Extensions.Logging;

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