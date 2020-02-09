namespace Framework.Api.Base.Errors
{
    using System;
    using Framework.Base.Errors;

    /*
     * A framework base class for error handling
     */
    internal class ErrorUtils
    {
        /*
         * Return a known error from a general exception
         */
        public static Exception FromException(Exception exception)
        {
            // If it is already a known error type then return that
            var apiError = ErrorUtils.TryConvertToApiError(exception);
            if (apiError != null)
            {
                return apiError;
            }

            var clientError = ErrorUtils.TryConvertToClientError(exception);
            if (clientError != null)
            {
                return clientError;
            }

            // Otherwise create a generic API error
            return ErrorUtils.CreateApiError(exception, null, null);
        }

        /*
         * Create an error from an exception with an error code and message
         */
        public static ApiError CreateApiError(Exception exception, string errorCode, string message)
        {
            var defaultErrorCode = BaseErrorCodes.ServerError;
            var defaultMessage = "An unexpected exception occurred in the API";

            // Create a default error and set a default technical message
            // To customise details instead, application code should use error translation and throw an ApiError
            var error = ErrorFactory.CreateApiError(
                    errorCode == null ? defaultErrorCode : errorCode,
                    message == null ? defaultMessage : message,
                    exception);

            // Set technical details
            error.SetDetails(exception.Message);
            return error;
        }

        /*
         * Handle unexpected data errors if an expected claim was not found in an OAuth message
         */
        public static ApiError FromMissingClaim(string claimName)
        {
            var error = ErrorFactory.CreateApiError("claims_failure", "Authorization data not found");
            error.SetDetails($"An empty value was found for the expected claim {claimName}");
            return error;
        }

        /*
         * Try to convert an exception to an API error
         */
        private static ApiError TryConvertToApiError(Exception exception)
        {
            // Direct conversions
            if (exception is ApiError)
            {
                return exception as ApiError;
            }

            // Also handle our extended exception type
            if (exception is ExtendedException)
            {
                return ErrorUtils.FromExtendedException((ExtendedException)exception);
            }

            // Check inner exceptions contained in async exceptions
            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is ApiError)
                {
                    return inner as ApiError;
                }

                if (inner is ExtendedException)
                {
                    return ErrorUtils.FromExtendedException((ExtendedException)inner);
                }

                inner = inner.InnerException;
            }

            return null;
        }

        /*
         * Try to convert an exception to a client error
         */
        private static ClientError TryConvertToClientError(Exception exception)
        {
            if (exception is ClientError)
            {
                return exception as ClientError;
            }

            // Check inner exceptions contained in async exceptions
            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is ClientError)
                {
                    return inner as ClientError;
                }

                inner = inner.InnerException;
            }

            return null;
        }

        /*
         * Convert from our custom runtime exception to an API error
         */
        private static ApiError FromExtendedException(ExtendedException ex)
        {
            var apiError = ErrorFactory.CreateApiError(ex.ErrorCode, ex.Message, ex);

            if (!string.IsNullOrWhiteSpace(ex.Details))
            {
                apiError.SetDetails(ex.Details);
            }

            return apiError;
        }
    }
}
