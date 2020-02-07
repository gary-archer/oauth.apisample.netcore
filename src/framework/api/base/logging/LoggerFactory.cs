namespace Framework.Api.Base.Logging
{
    using System;
    using Framework.Api.Base.Errors;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
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
            var repository = LogManager.CreateRepository($"{InstanceName}Repository", typeof(Hierarchy));

            /* Uncomment to view internal messages such as problems creating log files
            log4net.Util.LogLog.InternalDebugging = true;
            */

            // Set up log4net appenders
            // https://blogs.perficient.com/2016/04/20/how-to-programmatically-create-log-instance-by-log4net-library/
            var consoleAppender = this.CreateConsoleAppender();
            var fileAppender = this.CreateFileAppender();
            BasicConfigurator.Configure(repository, consoleAppender, fileAppender);
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
         * Create a console appender that uses JSON with pretty printing
         */
        private IAppender CreateConsoleAppender()
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
        private IAppender CreateFileAppender()
        {
            var jsonLayout = new JsonLayout(false);
            var fileAppender = new RollingFileAppender();
            fileAppender.File = $"./logs/";
            fileAppender.StaticLogFileName = false;
            fileAppender.DatePattern = "yyyy-MM-dd'.log'";
            fileAppender.AppendToFile = true;
            fileAppender.PreserveLogFileNameExtension = true;
            fileAppender.Layout = jsonLayout;
            fileAppender.LockingModel = new FileAppender.MinimalLock();
            fileAppender.MaximumFileSize = "1GB";
            fileAppender.MaxSizeRollBackups = -1;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            fileAppender.ActivateOptions();
            return fileAppender;
        }

        /*
         * Parse a log level from the configuration file
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