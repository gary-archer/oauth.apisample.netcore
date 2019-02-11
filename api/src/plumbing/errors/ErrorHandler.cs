namespace BasicApi.Plumbing.Errors
{
    using System;
    using IdentityModel.Client;
    using Microsoft.Extensions.Logging;
    using BasicApi.Entities;

    /*
     * Our error handler class
     */
    public class ErrorHandler
    {
        /*
         * Handle errors that prevent startup
         */
        public void HandleStartupException(Exception exception, ILogger logger)
        {
            var handledError = (ApiError)this.FromException(exception);
            var errorToLog = handledError.ToLogFormat();
            logger.LogError(errorToLog.ToString());
        }

        /*
         * Do server side error handling then return an error to the client
         */
        public ClientError HandleError(Exception exception, ILogger logger)
        {
            // Ensure that the exception has a known type
            var handledError = this.FromException(exception);
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
        public Exception FromException(Exception exception)
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

            // Also handle nested exceptions, including those during async completion or application startup
            if (exception is AggregateException)
            {
                if (exception.InnerException is ApiError)
                {
                    return exception.InnerException;
                }

                if (exception.InnerException is ClientError)
                {
                    return exception.InnerException;
                }

                // Get the underlying exception if possible, for a shorter and clearer error message
                if (exception.InnerException != null) 
                {
                    exception = exception.InnerException;
                }
            }
            
            // For other exceptions  we create a new object
            return new ApiError("general_exception", "An unexpected exception occurred in the API")
            {
                Details = exception.ToString(),
                Stack = exception.StackTrace
            };
        }

        /*
         * Report metadata lookup failures
         */
        public ApiError FromMetadataError(DiscoveryResponse response, string url)
        {
            return new ApiError("metadata_lookup_failure", "Metadata lookup failed")
            {
                Details = this.GetErrorDetails(response.Error, url)
            };
        }

        /*
         * Report introspection failures
         */
        public ApiError FromIntrospectionError(IntrospectionResponse response, string url)
        {
            return new ApiError("introspection_failure", "Token validation failed")
            {
                Details = this.GetErrorDetails(response.Error, url)
            };
        }

        /*
         * Report user info lookup failures
         */
        public ApiError FromUserInfoError(UserInfoResponse response, string url)
        {
            return new ApiError("userinfo_failure", "User info lookup failed")
            {
                Details = this.GetErrorDetails(response.Error, url)
            };
        }

        /*
         * Report missing claim failures
         */
        public ApiError FromMissingClaim(string claimName)
        {
            return new ApiError("claims_failure", "Authorization data not found")
            {
                Details = $"An empty value was found for the expected claim {claimName}"
            };
        }

        /*
         * A helper to concatenate details text
         */
        private string GetErrorDetails(string details, string url)
        {
            var detailsText = string.Empty;
            if (!string.IsNullOrWhiteSpace(details)) 
            {
                detailsText += details;
            }
        
            if(!string.IsNullOrWhiteSpace(url)) 
            {
                detailsText += $", URL: {url}";
            }

            return detailsText;
        }
    }
}
