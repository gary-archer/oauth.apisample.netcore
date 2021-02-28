namespace SampleApi.Plumbing.Logging
{
    /*
     * A simple builder class to expose a segregated interface
     */
    public static class LoggerFactoryBuilder
    {
        public static ILoggerFactory Create()
        {
            return new LoggerFactory();
        }
    }
}