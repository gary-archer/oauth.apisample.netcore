namespace BasicApi.Plumbing.Errors
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using BasicApi.Plumbing.Utilities;

    /*
     * Our unhandled exception handler
     */
    public class UnhandledExceptionMiddleware
    {
        /*
         * Injected properties
         */
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        /*
         * Store a reference to the next middleware
         */
        public UnhandledExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory.CreateLogger<UnhandledExceptionMiddleware>();
        }

        /*
         * Every request goes via this middleware
         */
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Run the API operation
                await this.next(context);
            }
            catch (Exception exception)
            {
                // Log full error details and return a less detailed error to the caller
                var clientError = ErrorHandler.HandleError(exception, logger);
                await ResponseErrorWriter.WriteErrorResponse(context, clientError.StatusCode, clientError.ToResponseFormat());
            }
        }
    }
}
