namespace SampleApi.Plumbing.Errors
{
    using System;

    /*
     * A class to manage error translation
     */
    public static class ErrorUtils
    {
        /*
         * Translate a general exception to a known format
         */
        public static Exception FromException(Exception exception)
        {
            // Avoid reprocessing
            var serverError = TryConvertToServerError(exception);
            if (serverError != null)
            {
                return serverError;
            }

            var clientError = TryConvertToClientError(exception);
            if (clientError != null)
            {
                return clientError;
            }

            // Otherwise create a generic server error
            return CreateServerError(exception, null, null);
        }

        /*
         * Create an error from an exception with an error code and message
         */
        public static ServerError CreateServerError(Exception exception, string errorCode, string message)
        {
            // Set defaults
            var defaultErrorCode = ErrorCodes.ServerError;
            var defaultMessage = "An unexpected exception occurred in the API";

            // Create a default error and set a default technical message
            return ErrorFactory.CreateServerError(
                errorCode == null ? defaultErrorCode : errorCode,
                message == null ? defaultMessage : message,
                exception);
        }

        /*
         * Handle failures in JWKS key responses
         */
        public static ServerError FromTokenSigningKeysDownloadError(int status, string url)
        {
            var error = ErrorFactory.CreateServerError(
                ErrorCodes.TokenSigningKeysDownloadError,
                "Problem encountered downloading JWKS keys");
            error.SetDetails($"Status: {status}, URL: {url}");
            return error;
        }

        /*
         * Report connectivity exceptions trying to downloading JWKS keys
         */
        public static ServerError FromTokenSigningKeysDownloadError(Exception ex, string url)
        {
            var serverError = TryConvertToServerError(ex);
            if (serverError != null)
            {
                return serverError;
            }

            return CreateServerError(ex, ErrorCodes.TokenSigningKeysDownloadError,  "Problem encountered downloading JWKS keys");
        }

        /*
         * Handle errors validating tokens
         */
        public static Exception FromTokenValidationError(Exception ex)
        {
            // Avoid reprocessing
            var serverError = TryConvertToServerError(ex);
            if (serverError != null)
            {
                return serverError;
            }

            var clientError = TryConvertToClientError(ex);
            if (clientError != null)
            {
                return clientError;
            }

            // Record the details behind the verification error
            var context = $"JWT verification failed: {ex.Message}";
            throw ErrorFactory.CreateClient401Error(context);
        }

        /*
         * Handle unexpected data errors if an expected claim was not found in an OAuth message
         */
        public static ClientError FromMissingClaim(string claimName)
        {
            return ErrorFactory.CreateClientErrorWithContext(
                System.Net.HttpStatusCode.BadRequest,
                ErrorCodes.InsufficientScope,
                "The token does not contain sufficient scope for this API",
                $"Missing claim in input: '{claimName}'");
        }

        /*
         * Try to convert an exception to a server error
         */
        private static ServerError TryConvertToServerError(Exception exception)
        {
            // Direct conversions
            if (exception is ServerError)
            {
                return exception as ServerError;
            }

            // Check inner exceptions contained in async exceptions
            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is ServerError)
                {
                    return inner as ServerError;
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
    }
}
