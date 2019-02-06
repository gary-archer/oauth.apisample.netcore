namespace BasicApi.Plumbing.Errors
{
    using System;
    using IdentityModel.Client;
    using Microsoft.Extensions.Logging;
    using BasicApi.Entities;

    /*
     * Our error handler class
     */
    public static class ErrorHandler
    {
        /*
         * Do server side error handling then return an error to the client
         */
        public static ClientError HandleError(Exception exception, ILogger logger)
        {
            // Ensure that the exception has a known type
            var handledError = FromException(exception);
            if (handledError is ClientError)
            {
                // Client errors mean the caller did something wrong
                var clientError = handledError as ClientError;

                // Log the error without an id
                var errorToLog = clientError.ToLogFormat();
                logger.LogError(errorToLog.ToString());

                // Return the typed error
                return clientError;
            }
            else
            {
                // API errors mean we experienced a failure
                var apiError = handledError as ApiError;
                
                // Log the error with an id
                var errorToLog = apiError.ToLogFormat();
                logger.LogError(errorToLog.ToString());

                // Return the client error to be returned
                return apiError.ToClientError();
            }
        }

        /*
         * Ensure that the exception has a known type
         */
        public static Exception FromException(Exception exception)
        {
            // Already handled 500 errors
            if (exception is ApiError)
            {
                return exception;
            }

            // Already handled 4xx errors
            if (exception is ClientError)
            {
                return exception as ClientError;
            }
            
            // For other exceptions  we create a new object
            return new ApiError("general_exception", "An unexpected exception occurred in the API")
            {
                Details = exception.ToString()
            };
        }

        /*
         * Report metadata lookup failures
         */
        public static ApiError FromMetadataError(DiscoveryResponse response, string url)
        {
            return new ApiError("metadata_lookup_failure", "Metadata lookup failed")
            {
                // StatusCode = (int)response.StatusCode,
                Details = response.Raw
            };

        }

        /*
         * Report user info lookup failures
         */
        public static ApiError FromUserInfoError(UserInfoResponse response, string url)
        {
            return new ApiError("userinfo_failure", "User info lookup failed")
            {
                // StatusCode = (int)response.HttpStatusCode,
                Details = response.Raw
            };
        }
    }
}
