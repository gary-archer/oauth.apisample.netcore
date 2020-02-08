namespace Framework.Api.Base.Middleware
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Logging;
    using Framework.Base.Abstractions;
    using Microsoft.AspNetCore.Http;

    /*
     * Middleware to do framework request logging
     */
    public class LoggerMiddleware
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
            logEntry.Write();
        }
    }
}
