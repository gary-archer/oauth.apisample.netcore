namespace Framework.Api.Base.Middleware
{
    using System;
    using System.Threading.Tasks;
    using Framework.Api.Base.Configuration;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Utilities;
    using Framework.Base.Abstractions;
    using Microsoft.AspNetCore.Http;

    /*
     * The unhandled exception handler, primarily called when an ASP.Net request fails
     */
    internal sealed class UnhandledExceptionMiddleware
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
         * The entry point for exceptions during API requests
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
                // Process the exception in a shared routine
                var clientError = this.HandleException(exception, context);

                // Write the error response to the client
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }

        /*
         * Do the work of handlign the exception in a shared routine
         */
        public ClientError HandleException(Exception exception, HttpContext context)
        {
            // Resolve dependencies used for error processing
            var logEntry = (LogEntry)context.RequestServices.GetService(typeof(ILogEntry));
            var configuration = (FrameworkConfiguration)context.RequestServices.GetService(typeof(FrameworkConfiguration));
            var applicationHandler = (ApplicationExceptionHandler)context.RequestServices.GetService(typeof(ApplicationExceptionHandler));

            // Allow the application to do its own translation if required
            var exceptionToHandle = exception;
            exceptionToHandle = applicationHandler.Translate(exceptionToHandle);

            // Get the error into a known object
            var error = ErrorUtils.FromException(exceptionToHandle);
            if (error is ApiError)
            {
                // Handle 5xx errors
                var apiError = (ApiError)error;
                logEntry.SetApiError(apiError);
                return apiError.ToClientError(configuration.ApiName);
            }
            else
            {
                // Handle 4xx errors
                ClientError clientError = (ClientError)error;
                logEntry.SetClientError(clientError);
                return clientError;
            }
        }
    }
}
