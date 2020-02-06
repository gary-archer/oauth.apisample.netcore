namespace Framework.Logging
{
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;

    /*
     * Set up log4net without a configuration file
     */
    public class LoggerFactory
    {
        public const string InstanceName = "Production";

        /*
         * Create the log4net setup to support JSON logging
         * https://blogs.perficient.com/2016/04/20/how-to-programmatically-create-log-instance-by-log4net-library/
         */
        public void ConfigureProductionRepository()
        {
            var repository = LogManager.CreateRepository($"{InstanceName}Repository", typeof(Hierarchy));

            /* Uncomment to view internal messages such as problems creating log files
            log4net.Util.LogLog.InternalDebugging = true;
            */

            var consoleAppender = this.CreateConsoleAppender();
            var fileAppender = this.CreateFileAppender();
            BasicConfigurator.Configure(repository, consoleAppender, fileAppender);
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