namespace Framework.Api.Base.Logging
{
    using System;
    using log4net;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /*
     * A logger factory interface
     */
    public interface ILoggerFactory
    {
        // The entry point for configuring logging
        void Configure(ILoggingBuilder builder, JObject loggingConfiguration);

        // Handle errors that prevent startup
        void LogStartupError(Exception exception);

        // Return the production logger to other classes in the framework
        ILog GetProductionLogger();
    }
}