namespace SampleApi.Host.Errors
{
    using System;
    using System.Threading.Tasks;
    using Framework.Errors;
    using Framework.Utilities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /*
     * The application exception handler
     */
    public class UnhandledExceptionHandler : BaseErrorHandler
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        /*
         * An override constructor for startup exceptions
         */
        public UnhandledExceptionHandler(ILogger logger)
        {
            this.next = null;
            this.logger = logger;
        }

        /*
         * Store a reference to the next middleware
         */
        public UnhandledExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory.CreateLogger<UnhandledExceptionHandler>();
        }

        /*
         * Handle errors that prevent startup, such as those downloading metadata
         */
        public void HandleStartupException(Exception exception)
        {
            this.HandleError(exception, this.logger);
        }

        /*
         * Controller exceptions are caught here
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
                var clientError = this.HandleError(exception, this.logger);
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    (int)clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }
    }
}
