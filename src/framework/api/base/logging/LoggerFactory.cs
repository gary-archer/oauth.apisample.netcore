namespace Framework.Api.Base.Logging
{
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /*
     * The entry point for configuring logging and getting a logger
     */
    public class LoggerFactory
    {
        private const string InstanceName = "Production";

        /*
         * The entry point for configuring logging
         */
        public void Configure(ILoggingBuilder builder, JObject loggingConfiguration)
        {
            this.ConfigureProductionLogging(builder);
            this.ConfigureDevelopmentTraceLogging(builder);
        }

        /*
         * Configure production logging, which works the same in all environments, to log queryable fields
         */
        public void ConfigureProductionLogging(ILoggingBuilder builder)
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
        public void ConfigureDevelopmentTraceLogging(ILoggingBuilder builder)
        {
            // Log info level except for exceptions
            builder
                .SetMinimumLevel(LogLevel.Information);

            // Avoid default noise from Microsoft libraries including base classes of particular classes
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error)
                .AddFilter("Framework.Api.Base.Security.CustomAuthenticationFilter", LogLevel.Error);

            // This can be set to debug level to capture logger per class data
            builder
                .AddFilter("Framework.Api.OAuth.Claims.ClaimsCache", LogLevel.Information);

            // Developer trace logging is only output to the console
            builder.AddConsole();
        }

        /*
         * Return the logger to other classes in the framework
         */
        public ILog GetProductionLogger()
        {
            return LogManager.GetLogger($"{InstanceName}Repository", $"{InstanceName}Logger");
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
    }
}