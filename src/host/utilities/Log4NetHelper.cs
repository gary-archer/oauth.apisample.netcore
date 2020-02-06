namespace SampleApi.Host.Utilities
{
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;

    /*
     * Set up log4net without a configuration file
     */
    public static class Log4NetHelper
    {
        public const string InstanceName = "Production";

        /*
         * Create the log4net setup to support JSON logging
         * https://blogs.perficient.com/2016/04/20/how-to-programmatically-create-log-instance-by-log4net-library/
         */
        public static void ConfigureProductionRepository()
        {
            var repository = LogManager.CreateRepository($"{InstanceName}Repository", typeof(Hierarchy));

            var patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            var consoleAppender = new ConsoleAppender();
            consoleAppender.Layout = patternLayout;

            BasicConfigurator.Configure(repository, consoleAppender);

            /*var fileAppender = new RollingFileAppender();
            fileAppender.AppendToFile = false;
            fileAppender.File = @"Logs\Log.txt";
            fileAppender.Layout = patternLayout;
            fileAppender.MaxSizeRollBackups = 5;
            fileAppender.MaximumFileSize = "1GB";
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            fileAppender.StaticLogFileName = true;
            fileAppender.ActivateOptions();*/
        }

        /*
         * A simple method to test logging
         */
        public static void TestLogging(string message)
        {
            var logger = LogManager.GetLogger($"{InstanceName}Repository", $"{InstanceName}Logger");
            logger.Warn("*** LOG4NET OUTPUT: " + message);
        }
    }
}