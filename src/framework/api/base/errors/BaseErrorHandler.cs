namespace Framework.Api.Base.Errors
{
    using System;
    using Framework.Api.Base.Logging;

    /*
     * A framework base class for error handling
     */
    public class BaseErrorHandler
    {
        /*
         * Do error handling and logging, then return an error to the client
         */
        public IClientError HandleError(Exception exception, LogEntry logEntry)
        {
            // Already handled API errors
            var apiError = this.TryConvertException<ApiError>(exception);
            if (apiError != null)
            {
                // Log the error, which will include technical support details
                logEntry.AddApiError(apiError);

                // Return a client error to the caller
                return apiError.ToClientError();
            }

            // If the API has thrown a 4xx error using an IClientError derived type then it is logged here
            var clientError = this.TryConvertException<IClientError>(exception);
            if (clientError != null)
            {
                // Log the error without an id
                logEntry.AddClientError(clientError);

                // Return the thrown error to the caller
                return clientError;
            }

            // Unhandled exceptions
            apiError = BaseErrorHandler.FromException(exception);
            logEntry.AddApiError(apiError);
            return apiError.ToClientError();
        }

        /*
         * A default implementation for creating an API error from an unrecognised exception
         */
        protected static ApiError FromException(Exception ex)
        {
            // Get the exception to use
            var exception = ex;
            if (ex is AggregateException)
            {
                if (ex.InnerException != null)
                {
                    exception = ex.InnerException;
                }
            }

            // Create a generic exception API error and note that in .Net the call stack is included in the details
            return new ApiError("server_error", "An unexpected exception occurred in the API")
            {
                Details = exception.ToString(),
            };
        }

        /*
         * Try to convert an exception to a known type
         */
        protected T TryConvertException<T>(Exception exception)
            where T : class
        {
            if (typeof(T).IsAssignableFrom(exception.GetType()))
            {
                return exception as T;
            }

            if (exception is AggregateException)
            {
                if (typeof(T).IsAssignableFrom(exception.InnerException.GetType()))
                {
                    return exception as T;
                }
            }

            return null;
        }
    }
}
