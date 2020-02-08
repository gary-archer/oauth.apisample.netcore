namespace Framework.Api.Base.Middleware
{
    using System;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Utilities;
    using Framework.Base.Abstractions;
    using Microsoft.AspNetCore.Http;

    /*
     * The application exception handler
     */
    public class UnhandledExceptionMiddleware
    {
        private readonly RequestDelegate next;

        /*
         * An overridden constructor for startup exceptions
         */
        public UnhandledExceptionMiddleware()
        {
            this.next = null;
        }

        /*
         * The usual middleware constructor
         */
        public UnhandledExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /*
         * Controller exceptions are caught here
         */
        public async Task Invoke(HttpContext context, ILogEntry logEntryParam)
        {
            try
            {
                // Run the API operation
                await this.next(context);
            }
            catch (Exception exception)
            {
                // Handle the error
                var logEntry = (LogEntry)logEntryParam;
                var handler = new ErrorUtils();
                var clientError = handler.HandleError(exception, logEntry);

                // Log full error details and return a less detailed error to the caller
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    (int)clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }
    }
}
