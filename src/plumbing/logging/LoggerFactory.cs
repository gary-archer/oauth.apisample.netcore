namespace SampleApi.Plumbing.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;

    /*
     * The entry point for configuring logging and getting a logger
     */
    internal sealed class LoggerFactory : ILoggerFactory
    {
        private static readonly string InstanceName = "Production";
        private readonly IList<PerformanceThreshold> thresholdOverrides;
        private string apiName;
        private bool isInitialized;
        private int defaultPerformanceThresholdMilliseconds;

        public LoggerFactory()
        {
            this.thresholdOverrides = new List<PerformanceThreshold>();
            this.apiName = string.Empty;
            this.isInitialized = false;
            this.defaultPerformanceThresholdMilliseconds = 0;
        }

        /*
         * The entry point for configuring logging
         */
        public void Configure(ILoggingBuilder builder, LoggingConfiguration configuration)
        {
            this.apiName = configuration.ApiName;
            this.ConfigureProductionLogging(builder, configuration.Production);
            this.ConfigureDevelopmentTraceLogging(builder, configuration.Development);
            this.isInitialized = true;
        }

        /*
         * Handle errors that prevent startup
         */
        public void LogStartupError(Exception exception)
        {
            if (this.isInitialized)
            {
                // Get the error into a loggable format
                var error = (ServerError)ErrorUtils.FromException(exception);

                // Output via log4net
                var logEntry = new LogEntry(this.apiName, this.GetProductionLogger());
                logEntry.SetOperationName("startup");
                logEntry.SetServerError(error);
                logEntry.Write();
            }
            else
            {
                // If logging is not set up yet use a plain exception dump
#pragma warning disable S2228
                Console.WriteLine($"STARTUP ERROR : {exception}");
#pragma warning restore S2228
            }
        }

        /*
         * Create the log entry and initialise it with data
         */
        public LogEntry CreateLogEntry()
        {
            return new LogEntry(this.apiName, this.GetProductionLogger(), this.GetPerformanceThreshold);
        }

        /*
         * Configure production logging, which works the same in all environments, to log queryable fields
         * https://blogs.perficient.com/2016/04/20/how-to-programmatically-create-log-instance-by-log4net-library/
         */
        private void ConfigureProductionLogging(ILoggingBuilder builder, JObject loggingConfiguration)
        {
            // Tell .Net Core to use log4net
            var options = new Log4NetProviderOptions
            {
                ExternalConfigurationSetup = true,
                UseWebOrAppConfig = false,
                LoggerRepository = InstanceName,
            };
            builder.AddLog4Net(options);

            // Create a repository for production JSON logging
            var repository = (Hierarchy)LogManager.CreateRepository($"{InstanceName}Repository", typeof(Hierarchy));
            repository.Root.Level = repository.LevelMap[loggingConfiguration["level"].ToString()];

#pragma warning disable S125
            /* Uncomment to view internal messages such as problems creating log files
            log4net.Util.LogLog.InternalDebugging = true;
            */
#pragma warning restore S125

            // Create appenders from configuration
            var appenders = this.CreateProductionAppenders((JArray)loggingConfiguration["appenders"]);
            BasicConfigurator.Configure(repository, appenders);

            // Load performance threshold data
            this.LoadPerformanceThresholds((JObject)loggingConfiguration["performanceThresholdsMilliseconds"]);
        }

        /*
         * The production log4net logger
         */
        private ILog GetProductionLogger()
        {
            return LogManager.GetLogger($"{InstanceName}Repository", $"{InstanceName}Logger");
        }

        /*
         * Use Microsoft .Net Core logging only for developer trace logging, which only ever runs on a developer PC
         * This logging is off by default
         */
        private void ConfigureDevelopmentTraceLogging(ILoggingBuilder builder, JObject loggingConfiguration)
        {
            // Set the base log level from configuration
            var level = this.ReadDevelopmentLogLevel(loggingConfiguration["level"].ToString());
            builder.SetMinimumLevel(level);

            // Process override levels
            var overrideLevels = (JObject)loggingConfiguration["overrideLevels"];
            if (overrideLevels != null)
            {
                foreach (var overrideLevel in overrideLevels)
                {
                    var className = overrideLevel.Key.ToString();
                    var classLevel = this.ReadDevelopmentLogLevel(overrideLevel.Value.ToString());
                    builder.AddFilter(className, classLevel);
                }
            }

            // Developer trace logging is only output to the console
            builder.AddConsole();
        }

        /*
         * Create appenders from configuration
         */
        private IAppender[] CreateProductionAppenders(JArray appendersConfiguration)
        {
            var appenders = new List<IAppender>();

            if (appendersConfiguration != null)
            {
                // Add the console appender if configured
                var consoleConfig = appendersConfiguration.Children<JObject>().FirstOrDefault(a => a["type"] != null && a["type"].ToString() == "console");
                if (consoleConfig != null)
                {
                    var consoleAppender = this.CreateProductionConsoleAppender(consoleConfig);
                    if (consoleAppender != null)
                    {
                        appenders.Add(consoleAppender);
                    }
                }

                // Add the file appender if configured
                var fileConfig = appendersConfiguration.Children<JObject>().FirstOrDefault(a => a["type"] != null && a["type"].ToString() == "file");
                if (fileConfig != null)
                {
                    var fileAppender = this.CreateProductionFileAppender(fileConfig);
                    if (fileAppender != null)
                    {
                        appenders.Add(fileAppender);
                    }
                }
            }

            return appenders.ToArray();
        }

        /*
         * Create a console appender that uses JSON with pretty printing
         */
        private IAppender CreateProductionConsoleAppender(JObject consoleConfiguration)
        {
            var prettyPrint = consoleConfiguration["prettyPrint"].ToObject<bool>();
            var jsonLayout = new JsonLayout(prettyPrint);
            jsonLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender();
            consoleAppender.Layout = jsonLayout;
            consoleAppender.ActivateOptions();
            return consoleAppender;
        }

        /*
         * Create a rolling file appender that uses JSON with an object per line
         * We use a new file per day and infinite backups of the form 2020-02-06.1.log
         */
        private IAppender CreateProductionFileAppender(JObject fileConfiguration)
        {
            // Get values
            var prefix = fileConfiguration["filePrefix"].ToString();
            var folder = fileConfiguration["dirName"].ToString();
            var maxSize = fileConfiguration["maxSize"].ToString();
            var maxFiles = fileConfiguration["maxFiles"].ToObject<int>();

            var jsonLayout = new JsonLayout(false);
            var fileAppender = new RollingFileAppender();
            fileAppender.File = folder;
            fileAppender.StaticLogFileName = false;
            fileAppender.DatePattern = $"{prefix}-yyyy-MM-dd'.log'";
            fileAppender.AppendToFile = true;
            fileAppender.PreserveLogFileNameExtension = true;
            fileAppender.Layout = jsonLayout;
            fileAppender.LockingModel = new FileAppender.MinimalLock();
            fileAppender.MaximumFileSize = maxSize;
            fileAppender.MaxSizeRollBackups = maxFiles;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            fileAppender.ActivateOptions();
            return fileAppender;
        }

        /*
         * Parse a log level that uses Microsoft logging
         */
        private LogLevel ReadDevelopmentLogLevel(string textValue)
        {
            LogLevel level;
            if (Enum.TryParse(textValue, true, out level))
            {
                return level;
            }

            return LogLevel.Information;
        }

        /*
         * Read any performance overrides into objects
         */
        private void LoadPerformanceThresholds(JObject thresholdData)
        {
            // Read the default
            this.defaultPerformanceThresholdMilliseconds = thresholdData["default"].ToObject<int>();

            // Process any overrides
            var operationOverrides = (JObject)thresholdData["operationOverrides"];
            if (operationOverrides != null)
            {
                foreach (var operationOverride in operationOverrides)
                {
                    var thresholdOverride = new PerformanceThreshold
                    {
                        Name = operationOverride.Key.ToString(),
                        Milliseconds = operationOverride.Value.ToObject<int>(),
                    };

                    this.thresholdOverrides.Add(thresholdOverride);
                }
            }
        }

        /*
         * Look up a performance threshold for an operation name
         */
        private int GetPerformanceThreshold(string operationName)
        {
            var found = this.thresholdOverrides.FirstOrDefault(o => o.Name.ToLowerInvariant() == operationName.ToLowerInvariant());
            if (found != null)
            {
                return found.Milliseconds;
            }

            return this.defaultPerformanceThresholdMilliseconds;
        }
    }
}