namespace Framework.Errors
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A framework base class for error handling
    /// </summary>
    public class BaseErrorHandler
    {
        /// <summary>
        /// Do error handling and logging, then return an error to the client
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="logger">The logger</param>
        /// <returns>An error to return to the API caller</returns>
        public IClientError HandleError(Exception exception, ILogger logger)
        {
            // Already handled API errors
            var apiError = this.TryConvertException<ApiError>(exception);
            if (apiError != null)
            {
                // Log the error, which will include technical support details
                logger.LogError(apiError.ToLogFormat().ToString());

                // Return a client error to the caller
                return apiError.ToClientError();
            }

            // If the API has thrown a 4xx error using an IClientError derived type then it is logged here
            var clientError = this.TryConvertException<IClientError>(exception);
            if (clientError != null)
            {
                // Log the error without an id
                logger.LogError(clientError.ToLogFormat().ToString());

                // Return the thrown error to the caller
                return clientError;
            }

            // Unhandled exceptions
            apiError = BaseErrorHandler.FromException(exception);
            logger.LogError(apiError.ToLogFormat().ToString());
            return apiError.ToClientError();
        }

        /// <summary>
        /// A default implementation for creating an API error from an unrecognised exception
        /// </summary>
        /// <param name="ex">The caught exception</param>
        /// <returns>The API error object in our standard format</returns>
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
                Details = exception.ToString()
            };
        }

        /// <summary>
        /// Try to convert an exception to a known type
        /// </summary>
        /// <typeparam name="T">The type of exception</typeparam>
        /// <param name="exception">The exception caught</param>
        /// <returns>The typed exception or null if the type does not match</returns>
        protected T TryConvertException<T>(Exception exception) where T : class
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
