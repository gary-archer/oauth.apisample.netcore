namespace Framework.Api.Base.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Api.Base.Errors;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository.Hierarchy;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /*
     * The entry point for configuring logging and getting a logger
     */
    public class LoggerFactory : ILoggerFactory
    {
        private const string InstanceName = "Production";
        private bool isInitialized = false;

        public LoggerFactory()
        {
            this.isInitialized = false;
        }

        /*
         * The entry point for configuring logging
         */
        public void Configure(ILoggingBuilder builder, JObject loggingConfiguration)
        {
            this.ConfigureProductionLogging(builder, (JObject)loggingConfiguration["production"]);
            this.ConfigureDevelopmentTraceLogging(builder, (JObject)loggingConfiguration["development"]);
            this.isInitialized = true;
        }

        /*
         * Handle errors that prevent startup, such as those downloading metadata or setting up logging
         */
        public void LogStartupError(Exception exception)
        {
            if (this.isInitialized)
            {
                // Use log4net output if we can
                var handler = new ErrorUtils();
                var logEntry = new LogEntry();
                handler.HandleError(exception, logEntry);
                logEntry.End(null);
            }
            else
            {
                // If logging is not set up yet use a plain exception dump
                Console.WriteLine($"STARTUP ERROR : {exception}");
            }
        }

        /*
         * Return the logger to other classes in the framework
         */
        public ILog GetProductionLogger()
        {
            return LogManager.GetLogger($"{InstanceName}Repository", $"{InstanceName}Logger");
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

            /* Uncomment to view internal messages such as problems creating log files
            log4net.Util.LogLog.InternalDebugging = true;
            */

            // Create appenders from configuration
            var appenders = this.CreateProductionAppenders((JArray)loggingConfiguration["appenders"]);
            BasicConfigurator.Configure(repository, appenders);
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

            System.Console.WriteLine("DEFAULT: " + level);

            // Process override levels
            var overrideLevels = (JObject)loggingConfiguration["overrideLevels"];
            if (overrideLevels != null)
            {
                foreach (var overrideLevel in overrideLevels)
                {
                    var className = overrideLevel.Key.ToString();
                    var classLevel = this.ReadDevelopmentLogLevel(overrideLevel.Value.ToString());
                    builder.AddFilter(className, classLevel);
                    System.Console.WriteLine(className + " : " + classLevel);
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
                    var consoleAppender = this.CreateProductionConsoleAppender();
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
        private IAppender CreateProductionConsoleAppender()
        {
            var jsonLayout = new JsonLayout(true);
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
            LogLevel level = LogLevel.Information;
            if (Enum.TryParse(textValue, true, out level))
            {
                return level;
            }

            return LogLevel.Information;
        }
    }
}