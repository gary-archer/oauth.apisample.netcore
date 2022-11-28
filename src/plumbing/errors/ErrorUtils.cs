namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Collections.Generic;

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
         * Report user info failures clearly
         */
        public static Exception FromUserInfoError(int status, string responseData, string url)
        {
            // Add base details
            var parts = new List<string>();
            parts.Add("User info lookup failed");
            parts.Add($"Status: {status}");

            // Read the standard OAuth error fields
            var json = ErrorResponseReader.ReadJson(responseData);
            if (json != null)
            {
                var code = json.GetValue("error");
                if (code != null)
                {
                    parts.Add($"Code: {code}");
                }

                var description = json.GetValue("error_description");
                if (description != null)
                {
                    parts.Add($"Description: {description}");
                }
            }

            // Finalize details
            parts.Add($"URL: {url}");
            var details = string.Join(", ", parts);

            // If there is a race condition and the access token is expired return a 401
            if (status == 401)
            {
                return ErrorFactory.CreateClient401Error(details);
            }

            // Otherwise return a 500
            var error = ErrorFactory.CreateServerError(
                ErrorCodes.UserInfoFailure,
                "User info lookup failed");
            error.SetDetails(details);
            return error;
        }

        /*
         * Report connectivity exceptions trying to downloading user info
         */
        public static Exception FromUserInfoError(Exception ex, string url)
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

            // Create the error
            return CreateServerError(ex, ErrorCodes.UserInfoFailure,  "User info lookup failed");
        }

        /*
         * Handle unexpected data errors if an expected claim was not found in an OAuth message
         */
        public static ClientError FromMissingClaim(string claimName)
        {
            return ErrorFactory.CreateClientErrorWithContext(
                System.Net.HttpStatusCode.BadRequest,
                ErrorCodes.ClaimsFailure,
                "Authorization data not found",
                $"Missing claim in input: {claimName}");
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
