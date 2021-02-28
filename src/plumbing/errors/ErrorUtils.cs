namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Collections.Generic;
    using IdentityModel.Client;
    using Newtonsoft.Json.Linq;

    /*
     * A class to manage error translation
     */
    internal static class ErrorUtils
    {
        /*
         * Translate a general exception to a known format
         */
        public static Exception FromException(Exception exception)
        {
            // Avoid reprocessing
            var serverError = ErrorUtils.TryConvertToServerError(exception);
            if (serverError != null)
            {
                return serverError;
            }

            var clientError = ErrorUtils.TryConvertToClientError(exception);
            if (clientError != null)
            {
                return clientError;
            }

            // Otherwise create a generic server error
            return ErrorUtils.CreateServerError(exception, null, null);
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
         * Report metadata failures clearly
         */
        public static ServerError FromMetadataError(DiscoveryDocumentResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var oauthError = ErrorUtils.CreateOAuthServerError(
                ErrorCodes.MetadataLookupFailure,
                "Metadata lookup failed",
                data.Item1);

            oauthError.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return oauthError;
        }

        /*
         * Report metadata lookup exceptions clearly
         */
        public static ServerError FromMetadataError(Exception ex, string url)
        {
            var error = ErrorUtils.CreateServerError(
                ex,
                ErrorCodes.MetadataLookupFailure,
                "Metadata lookup failed");
            error.SetDetails($"URL: {url}");
            return error;
        }

        /*
         * Report failures in introspection responses
         */
        public static ServerError FromIntrospectionError(TokenIntrospectionResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var error = ErrorUtils.CreateOAuthServerError(
                ErrorCodes.IntrospectionFailure,
                "Token validation failed",
                data.Item1);

            error.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return error;
        }

        /*
         * Handle exceptions when performing introspection
         */
        public static Exception FromIntrospectionError(Exception ex, string url)
        {
            // Avoid reprocessing
            var serverError = ErrorUtils.TryConvertToServerError(ex);
            if (serverError != null)
            {
                return serverError;
            }

            var clientError = ErrorUtils.TryConvertToClientError(ex);
            if (clientError != null)
            {
                return clientError;
            }

            var error = ErrorUtils.CreateServerError(ex, ErrorCodes.IntrospectionFailure, "Token validation failed");
            error.SetDetails($"URL: {url}");
            return error;
        }

        /*
         * Handle failures in JWKS key responses
         */
        public static ServerError FromTokenSigningKeysDownloadError(JsonWebKeySetResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var error = ErrorUtils.CreateOAuthServerError(
                ErrorCodes.TokenSigningKeysDownloadError,
                "Problem downloading JWKS keys",
                data.Item1);

            error.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return error;
        }

        /*
         * Handle exceptions downloading JWKS keys
         */
        public static ServerError FromTokenSigningKeysDownloadError(Exception ex, string url)
        {
            return ErrorUtils.CreateServerError(ex, ErrorCodes.TokenSigningKeysDownloadError,  "Problem downloading JWKS keys");
        }

        /*
         * Report user info failures clearly
         */
        public static Exception FromUserInfoError(UserInfoResponse response, string url)
        {
            // Read the OAuth error code
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);

            // Handle a race condition where the access token expires during user info lookup
            if (data.Item1 == ErrorCodes.UserInfoTokenExpired)
            {
                return ErrorFactory.CreateClient401Error("Access token expired during user info lookup");
            }

            // Report other errors
            var error = ErrorUtils.CreateOAuthServerError(
                ErrorCodes.UserInfoFailure,
                "User info lookup failed",
                data.Item1);
            error.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return error;
        }

        /*
         * Handle exceptions when getting User Info
         */
        public static Exception FromUserInfoError(Exception ex, string url)
        {
            // Avoid reprocessing
            var serverError = ErrorUtils.TryConvertToServerError(ex);
            if (serverError != null)
            {
                return serverError;
            }

            var clientError = ErrorUtils.TryConvertToClientError(ex);
            if (clientError != null)
            {
                return clientError;
            }

            // Create the error
            return ErrorUtils.CreateServerError(ex, ErrorCodes.UserInfoFailure,  "User info lookup failed");
        }

        /*
         * Handle unexpected data errors if an expected claim was not found in an OAuth message
         */
        public static ServerError FromMissingClaim(string claimName)
        {
            var error = ErrorFactory.CreateServerError("claims_failure", "Authorization data not found");
            error.SetDetails($"An empty value was found for the expected claim {claimName}");
            return error;
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

        /*
         * Return any OAuth protocol error details
         */
        private static Tuple<string, string> ReadOAuthErrorResponse(JObject jsonBody)
        {
            string code = null;
            string description = null;
            if (jsonBody != null)
            {
                code = jsonBody.TryGetString("error");
                description = jsonBody.TryGetString("error_description");
            }

            return Tuple.Create(code, description);
        }

        /*
         * Create an error object from an error code and include the OAuth error code in the user message
         */
        private static ServerError CreateOAuthServerError(string errorCode, string userMessage, string oauthErrorCode)
        {
            string message = userMessage;
            if (!string.IsNullOrWhiteSpace(oauthErrorCode))
            {
                message += $" : {oauthErrorCode}";
            }

            return ErrorFactory.CreateServerError(errorCode, message);
        }

        /*
         * A helper to concatenate error parts
         */
        private static string GetOAuthErrorDetails(string oauthErrorDescription, string details, string url)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(details))
            {
                parts.Add(details);
            }

            if (!string.IsNullOrWhiteSpace(oauthErrorDescription))
            {
                parts.Add(oauthErrorDescription);
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                parts.Add(url);
            }

            return string.Join(", ", parts);
        }
    }
}
