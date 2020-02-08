namespace Framework.Api.Base.Logging
{
    using System;
    using Framework.Api.Base.Configuration;
    using log4net;
    using Microsoft.Extensions.Logging;

    /*
     * A logger factory interface
     */
    public interface ILoggerFactory
    {
        // The entry point for configuring logging
        void Configure(ILoggingBuilder builder, FrameworkConfiguration configuration);

        // Handle errors that prevent startup
        void LogStartupError(Exception exception);
    }
}