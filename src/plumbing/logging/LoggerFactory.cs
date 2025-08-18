namespace FinalApi.Plumbing.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Nodes;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Errors;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;
    using Microsoft.Extensions.Logging;

    /*
     * The entry point for configuring logging and getting a logger
     * https://blogs.perficient.com/2016/04/20/how-to-programmatically-create-log-instance-by-log4net-library/
     */
    internal sealed class LoggerFactory : ILoggerFactory
    {
        private string apiName;
        private int performanceThresholdMilliseconds;
        private bool isInitialized;

        public LoggerFactory()
        {
            this.apiName = string.Empty;
            this.performanceThresholdMilliseconds = 1000;
            this.isInitialized = false;
        }

        /*
         * Configure logging at startup
         */
        public void Configure(ILoggingBuilder builder, LoggingConfiguration configuration)
        {
            this.apiName = configuration.ApiName;

            // Uncomment to view internal log4net errors
            // log4net.Util.LogLog.InternalDebugging = true;

            // Tell .NET to use log4net
            var options = new Log4NetProviderOptions
            {
                ExternalConfigurationSetup = true,
                UseWebOrAppConfig = false,
                LoggerRepository = "default",
            };
            builder.AddLog4Net(options);

            // Create the fixed request logger
            var requestLogConfig = configuration.Loggers.FirstOrDefault(l => l["type"]?.GetValue<string>() == "request");
            if (requestLogConfig != null)
            {
                this.performanceThresholdMilliseconds = requestLogConfig["performanceThresholdMilliseconds"].GetValue<int>();
                this.CreateRequestLogger(requestLogConfig);
            }

            // Create the fixed audit logger
            var auditLogConfig = configuration.Loggers.FirstOrDefault(l => l["type"]?.GetValue<string>() == "audit");
            if (auditLogConfig != null)
            {
                this.CreateAuditLogger(auditLogConfig);
            }

            // Create the fixed audit logger
            var debugLogConfig = configuration.Loggers.FirstOrDefault(l => l["type"]?.GetValue<string>() == "debug");
            if (debugLogConfig != null)
            {
                this.CreateDebugLoggers(builder, debugLogConfig);
            }

            this.isInitialized = true;
        }

        /*
         * Log API startup errors
         */
        public void LogStartupError(Exception exception)
        {
            if (this.isInitialized)
            {
                // Get the error into a loggable format
                var error = (ServerError)ErrorUtils.FromException(exception);

                // Output via log4net
                var logEntry = new LogEntry(this.apiName, this.performanceThresholdMilliseconds);
                logEntry.SetOperationName("startup");
                logEntry.SetServerError(error);
                this.GetRequestLogger()?.Info(logEntry.GetRequestLog());
            }
            else
            {
                // If logging is not set up yet use a plain exception dump
                Console.WriteLine($"STARTUP ERROR : {exception}");
            }
        }

        /*
         * Get the request logger
         */
        public ILog GetRequestLogger()
        {
            return LogManager.GetLogger("request", "requestLogger");
        }

        /*
         * Get the audit logger
         */
        public ILog GetAuditLogger()
        {
            return LogManager.GetLogger("audit", "auditLogger");
        }

        /*
         * Get a named debug logger
         */
        public ILog GetDebugLogger(string name)
        {
            return LogManager.GetLogger("debug", name);
        }

        /*
         * Create the log entry and initialise it with data
         */
        public LogEntry CreateLogEntry()
        {
            return new LogEntry(this.apiName, this.performanceThresholdMilliseconds);
        }

        /*
         * Add an always on request logger for technical support details
         */
        private void CreateRequestLogger(JsonNode config)
        {
            var repository = (Hierarchy)LogManager.CreateRepository("request", typeof(Hierarchy));
            repository.Root.Level = repository.LevelMap["Info"];

            var appenders = this.CreateAppenders(config["appenders"].AsArray());
            BasicConfigurator.Configure(repository, appenders);
        }

        /*
         * Add an always on audit logger for security details
         */
        private void CreateAuditLogger(JsonNode config)
        {
            var repository = (Hierarchy)LogManager.CreateRepository("audit", typeof(Hierarchy));
            repository.Root.Level = repository.LevelMap["Info"];

            var appenders = this.CreateAppenders(config["appenders"].AsArray());
            BasicConfigurator.Configure(repository, appenders);
        }

        /*
         * Create appenders from configuration
         */
        private IAppender[] CreateAppenders(JsonArray appendersConfiguration)
        {
            var appenders = new List<IAppender>();

            if (appendersConfiguration != null)
            {
                // Add the console appender if configured
                var consoleConfig = appendersConfiguration.FirstOrDefault(a => a["type"]?.GetValue<string>() == "console");
                if (consoleConfig != null)
                {
                    var consoleAppender = this.CreateConsoleAppender(consoleConfig);
                    if (consoleAppender != null)
                    {
                        appenders.Add(consoleAppender);
                    }
                }

                // Add the file appender if configured
                var fileConfig = appendersConfiguration.FirstOrDefault(a => a["type"]?.GetValue<string>() == "file");
                if (fileConfig != null)
                {
                    var fileAppender = this.CreateFileAppender(fileConfig);
                    if (fileAppender != null)
                    {
                        appenders.Add(fileAppender);
                    }
                }
            }

            return appenders.ToArray();
        }

        /*
         * Create a JSON console appender
         */
        private IAppender CreateConsoleAppender(JsonNode consoleConfiguration)
        {
            var prettyPrint = consoleConfiguration["prettyPrint"].GetValue<bool>();
            var jsonLayout = new JsonLayout(prettyPrint);
            jsonLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender()
            {
                Layout = jsonLayout,
            };

            consoleAppender.ActivateOptions();
            return consoleAppender;
        }

        /*
         * Create a rolling file appender that uses JSON with an object per line
         * We use a new file per day and infinite backups of the form 2020-02-06.1.log
         */
        private IAppender CreateFileAppender(JsonNode fileConfiguration)
        {
            // Get values
            var prefix = fileConfiguration["filePrefix"].GetValue<string>();
            var folder = fileConfiguration["dirName"].GetValue<string>();
            var maxSize = fileConfiguration["maxSize"].GetValue<string>();
            var maxFiles = fileConfiguration["maxFiles"].GetValue<int>();

            var jsonLayout = new JsonLayout(false);
            var fileAppender = new RollingFileAppender()
            {
                File = $"{folder}/{prefix}-",
                StaticLogFileName = false,
                DatePattern = "yyyy-MM-dd.'log'",
                AppendToFile = true,
                PreserveLogFileNameExtension = true,
                Layout = jsonLayout,
                LockingModel = new FileAppender.MinimalLock(),
                MaximumFileSize = maxSize,
                MaxSizeRollBackups = maxFiles,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
            };

            fileAppender.ActivateOptions();
            return fileAppender;
        }

        /*
         * Add debug loggers
         */
        private void CreateDebugLoggers(ILoggingBuilder builder, JsonNode config)
        {
            // Set the base log level from configuration
            var level = this.ReadDebugLogLevel(config["level"].GetValue<string>());
            builder.SetMinimumLevel(level);

            // Process override levels
            var overrideLevels = config["overrideLevels"]?.AsObject();
            if (overrideLevels != null)
            {
                foreach (var overrideLevel in overrideLevels)
                {
                    var className = overrideLevel.Key;
                    var classLevel = this.ReadDebugLogLevel(overrideLevel.Value.GetValue<string>());
                    builder.AddFilter(className, classLevel);
                }
            }

            // Developer trace logging is only output to the console
            builder.AddConsole();
        }

        /*
         * Parse a log level that uses Microsoft logging
         */
        private LogLevel ReadDebugLogLevel(string textValue)
        {
            LogLevel level;
            if (Enum.TryParse(textValue, true, out level))
            {
                return level;
            }

            return LogLevel.Information;
        }
    }
}
