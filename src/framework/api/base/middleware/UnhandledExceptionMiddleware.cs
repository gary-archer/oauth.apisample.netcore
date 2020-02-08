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
        public async Task Invoke(
            HttpContext context,
            FrameworkConfiguration configuration,
            ILogEntry logEntryParam,
            ApplicationExceptionHandler applicationHandler)
        {
            try
            {
                // Run the API operation
                await this.next(context);
            }
            catch (Exception exception)
            {
                // Process the entry
                var logEntry = (LogEntry)logEntryParam;
                var clientError = this.HandleError(
                    exception,
                    configuration,
                    logEntry,
                    applicationHandler);

                // Write the error response to the client
                await ResponseErrorWriter.WriteErrorResponse(
                    context.Request,
                    context.Response,
                    clientError.StatusCode,
                    clientError.ToResponseFormat());
            }
        }

        /*
         * A shared routine also called for authentication errors
         */
        public ClientError HandleError(
            Exception exception,
            FrameworkConfiguration configuration,
            LogEntry logEntry,
            ApplicationExceptionHandler applicationHandler)
        {
            // Move past aggregate exceptions
            var exceptionToHandle = exception;
            if (exceptionToHandle is AggregateException)
            {
                if (exceptionToHandle.InnerException != null)
                {
                    exceptionToHandle = exceptionToHandle.InnerException;
                }
            }

            // Allow the application to implement its own error logic if required
            if (applicationHandler != null)
            {
                exceptionToHandle = applicationHandler.Translate(exceptionToHandle);
            }

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
