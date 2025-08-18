namespace FinalApi.Plumbing.Middleware
{
    using System.Threading.Tasks;
    using FinalApi.Plumbing.Logging;
    using Microsoft.AspNetCore.Http;

    /*
     * Middleware to do API request logging
     */
    public sealed class LoggerMiddleware
    {
        private readonly RequestDelegate next;

        /*
         * Store a reference to the next middleware
         */
        public LoggerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /*
         * Log the request and response
         */
        public async Task Invoke(HttpContext context, ILogEntry logEntryParam)
        {
            // Start logging request details
            var logEntry = (LogEntry)logEntryParam;
            logEntry.Start(context.Request);

            // Run subsequent handlers including the controller operation
            await this.next(context);

            // Log response details before exiting
            logEntry.End(context.Request, context.Response);

            // GJA
            // logEntry.Write();
            /*


            // Get the object to log
            var logData = this.data.ToLogFormat();

            // Output it
            if (item.ErrorData != null)
            {
                this.productionLogger.Error(logData);
            }
            else
            {
                this.productionLogger.Info(logData);
            }
            */
        }
    }
}
