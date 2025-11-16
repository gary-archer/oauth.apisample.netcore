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

            // Add response details to logs
            logEntry.End(context.Request, context.Response);

            // Output log data
            var loggerFactory = (ILoggerFactory)context.RequestServices.GetService(typeof(LoggerFactory));
            loggerFactory.GetRequestLogger()?.Info(logEntry.GetRequestLog());
            loggerFactory.GetAuditLogger()?.Info(logEntry.GetAuditLog());
        }
    }
}
