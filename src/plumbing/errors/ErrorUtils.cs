namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Collections.Generic;
    using IdentityModel.Client;
    using Newtonsoft.Json.Linq;

    /*
     * A class to manage error translation
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
            var defaultErrorCode = ErrorCodes.ServerError;
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
         * Report metadata lookup failures clearly
         */
        public static ApiError FromMetadataError(DiscoveryDocumentResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var apiError = ErrorUtils.CreateOAuthApiError(
                ErrorCodes.MetadataLookupFailure,
                "Metadata lookup failed",
                data.Item1);

            apiError.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return apiError;
        }

        /*
         * Report introspection failures clearly
         */
        public static ApiError FromIntrospectionError(TokenIntrospectionResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var apiError = ErrorUtils.CreateOAuthApiError(
                ErrorCodes.IntrospectionFailure,
                "Token validation failed",
                data.Item1);

            apiError.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return apiError;
        }

        /*
         * Report user info failures clearly
         */
        public static ApiError FromUserInfoError(UserInfoResponse response, string url)
        {
            var data = ErrorUtils.ReadOAuthErrorResponse(response.Json);
            var apiError = ErrorUtils.CreateOAuthApiError(
                ErrorCodes.UserInfoFailure,
                "User info lookup failed",
                data.Item1);

            apiError.SetDetails(ErrorUtils.GetOAuthErrorDetails(data.Item2, response.Error, url));
            return apiError;
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

            // Check inner exceptions contained in async exceptions
            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is ApiError)
                {
                    return inner as ApiError;
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
        private static ApiError CreateOAuthApiError(string errorCode, string userMessage, string oauthErrorCode)
        {
            string message = userMessage;
            if (!string.IsNullOrWhiteSpace(oauthErrorCode))
            {
                message += $" : {oauthErrorCode}";
            }

            return ErrorFactory.CreateApiError(errorCode, message);
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
