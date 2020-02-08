namespace Framework.Api.Base.Middleware
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Logging;
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
        public async Task Invoke(HttpContext context, LogEntry logEntry, ILoggerFactory loggerFactory)
        {
            // Start logging request details
            logEntry.Start(context.Request);

            // Run subsequent handlers including the controller operation
            await this.next(context);

            // Log response details before exiting
            logEntry.End(context.Response, loggerFactory.GetProductionLogger());
        }
    }
}
