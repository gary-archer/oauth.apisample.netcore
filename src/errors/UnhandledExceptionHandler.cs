namespace BasicApi.Errors
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Framework.Utilities;
    using Framework.Errors;

    /// <summary>
    /// The application exception handler
    /// </summary>
    public class UnhandledExceptionHandler : BaseErrorHandler
    {
        // Injected properties
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        /// <summary>
        /// An override constructor for startup exceptions
        /// </summary>
        /// <param name="logger">The logger</param>
        public UnhandledExceptionHandler(ILogger logger)
        {
            this.next = null;
            this.logger = logger;
        }

        /// <summary>
        /// Store a reference to the next middleware
        /// </summary>
        /// <param name="next">The next middleware</param>
        /// <param name="loggerFactory">The logger factory</param>
        public UnhandledExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory.CreateLogger<UnhandledExceptionHandler>();
        }

        /// <summary>
        /// Handle errors that prevent startup, such as those downloading metadata
        /// </summary>
        /// <param name="exception">The exception</param>
        public void HandleStartupException(Exception exception)
        {
            base.HandleError(exception, this.logger);
        }

        /// <summary>
        /// Controller exceptions are caught here
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>A task to await</returns>
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
                var clientError = this.HandleError(exception, logger);
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    (int)clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }
    }
}
