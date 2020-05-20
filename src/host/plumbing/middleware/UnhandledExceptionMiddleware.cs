namespace SampleApi.Host.Plumbing.Middleware
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Host.Plumbing.Configuration;
    using SampleApi.Host.Plumbing.Errors;
    using SampleApi.Host.Plumbing.Logging;
    using SampleApi.Host.Plumbing.Utilities;

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
            var configuration = (LoggingConfiguration)context.RequestServices.GetService(typeof(LoggingConfiguration));

            // Get the error into a known object
            var error = ErrorUtils.FromException(exception);
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
