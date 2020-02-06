namespace Framework.Api.Base.Middleware
{
    using System;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Utilities;
    using Microsoft.AspNetCore.Http;

    /*
     * The application exception handler
     */
    public class UnhandledExceptionMiddleware : BaseErrorHandler
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
         * Handle errors that prevent startup, such as those downloading metadata
         */
        public void HandleStartupException(Exception exception)
        {
            var logEntry = new LogEntry();
            this.HandleError(exception, logEntry);
            logEntry.End(null);
        }

        /*
         * Controller exceptions are caught here
         */
        public async Task Invoke(HttpContext context, LogEntry logEntry)
        {
            try
            {
                // Run the API operation
                await this.next(context);
            }
            catch (Exception exception)
            {
                // Log full error details and return a less detailed error to the caller
                var clientError = this.HandleError(exception, logEntry);
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    (int)clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }
    }
}
