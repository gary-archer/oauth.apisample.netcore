namespace Framework.Errors
{
    using System;
    using System.Collections.Generic;
    using IdentityModel.Client;
    using Newtonsoft.Json.Linq;

    /*
     * Framework specific error handling
     */
    public sealed class OAuthErrorHandler : BaseErrorHandler
    {
        /*
         * Report metadata lookup failures clearly
         */
        public ApiError FromMetadataError(DiscoveryResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("metadata_lookup_failure", "Metadata lookup failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /*
         * Report introspection failures clearly
         */
        public ApiError FromIntrospectionError(IntrospectionResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("introspection_failure", "Token validation failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /*
         * Report user info failures clearly
         */
        public ApiError FromUserInfoError(UserInfoResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("userinfo_failure", "User info lookup failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /*
         * Handle unexpected data errors if an expected claim was not found in an OAuth message
         */
        public ApiError FromMissingClaim(string claimName)
        {
            return new ApiError("claims_failure", "Authorization data not found")
            {
                Details = $"An empty value was found for the expected claim {claimName}"
            };
        }

        /*
         * Return any OAuth protocol error details
         */
        private Tuple<string, string> ReadOAuthErrorResponse(JObject jsonBody)
        {
            string code = null;
            string description = null;
            if(jsonBody != null)
            {
                code = jsonBody.TryGetString("error");
                description = jsonBody.TryGetString("error_description");
            }

            return Tuple.Create(code, description);
        }

        /*
         * Create an error object from an error code and include the OAuth error code in the user message
         */
        private ApiError CreateOAuthApiError(string errorCode, string userMessage, string oauthErrorCode)
        {
            string message = userMessage;
            if (!string.IsNullOrWhiteSpace(oauthErrorCode)) {
                message += $" : {oauthErrorCode}";
            }

            return new ApiError(errorCode, message);
        }

        /*
         * A helper to concatenate error parts
         */
        private string GetErrorDetails(string oauthErrorDescription, string details, string url)
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

            return string.Join(", ",parts);
        }
    }
}