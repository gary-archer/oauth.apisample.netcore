namespace SampleApi.Host.Utilities
{
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    
    /*
     * Set up log4net without a configuration file
     */
    public static class Log4NetHelper
    {
        public static string ProductionRepository = "PRODUCTION";

        /*
         * Create the log4net setup to support JSON logging
         */
        public static void ConfigureProductionRepository()
        {
            var hierarchy = (Hierarchy)LogManager.CreateRepository(ProductionRepository, typeof(Hierarchy));

            var patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            /*var roller = new RollingFileAppender();
            roller.AppendToFile = false;
            roller.File = @"Logs\Log.txt";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "1GB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;            
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);*/

            var console = new ConsoleAppender();
            hierarchy.Root.AddAppender(console);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;

            System.Console.WriteLine("ROOT LOGGER IS");
            System.Console.WriteLine(hierarchy.Root.Name);
        }

        /*
         * A simple method to test logging
         */
        public static void TestLogging()
        {
            System.Console.WriteLine("LOGGING");
            var logger = LogManager.GetLogger(ProductionRepository, "root");
            logger.Warn("HELLO FROM LOG4NET");
            System.Console.WriteLine("LOGGED");
        }
    }
}