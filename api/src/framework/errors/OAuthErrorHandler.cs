namespace Framework.Errors
{
    using System;
    using System.Collections.Generic;
    using IdentityModel.Client;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Framework specific error handling
    /// </summary>
    public sealed class OAuthErrorHandler : BaseErrorHandler
    {
        /// <summary>
        /// Report metadata lookup failures clearly
        /// </summary>
        /// <param name="response">The response</param>
        /// <param name="url">The URL we called</param>
        /// <returns>Details to log specific to this scenario</returns>
        public ApiError FromMetadataError(DiscoveryResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("metadata_lookup_failure", "Metadata lookup failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /// <summary>
        /// Report introspection failures clearly
        /// </summary>
        /// <param name="response">The response</param>
        /// <param name="url">The URL we called</param>
        /// <returns>Details to log specific to this scenario</returns>
        public ApiError FromIntrospectionError(IntrospectionResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("introspection_failure", "Token validation failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /// <summary>
        /// Report user info failures clearly
        /// </summary>
        /// <param name="response">The response</param>
        /// <param name="url">The URL we called</param>
        /// <returns>Details to log specific to this scenario</returns>
        public ApiError FromUserInfoError(UserInfoResponse response, string url)
        {
            var data = this.ReadOAuthErrorResponse(response.Json);
            var apiError = CreateOAuthApiError("userinfo_failure", "User info lookup failed", data.Item1);
            apiError.Details = this.GetErrorDetails(data.Item2, response.Error, url);
            return apiError;
        }

        /// <summary>
        /// Handle unexpected data errors if an expected claim was not found in an OAuth message
        /// </summary>
        /// <param name="claimName"></param>
        /// <returns></returns>
        public ApiError FromMissingClaim(string claimName)
        {
            return new ApiError("claims_failure", "Authorization data not found")
            {
                Details = $"An empty value was found for the expected claim {claimName}"
            };
        }

        /// <summary>
        /// Return any OAuth protocol error details
        /// </summary>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create an error object from an error code and include the OAuth error code in the user message
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="userMessage">A short technical messaage for the client</param>
        /// <param name="oauthErrorCode">The OAuth error code if present</param>
        /// <returns></returns>
        private ApiError CreateOAuthApiError(string errorCode, string userMessage, string oauthErrorCode)
        {
            string message = userMessage;
            if (!string.IsNullOrWhiteSpace(oauthErrorCode)) {
                message += $" : {oauthErrorCode}";
            }

            return new ApiError(errorCode, message);
        }

        /// <summary>
        /// A helper to concatenate error parts
        /// </summary>
        /// <param name="oauthErrorDescription">The OAuth error description if present</param>
        /// <param name="details">Error details</param>
        /// <param name="url">The URL where the problem occurred</param>
        /// <returns>A concatenated details string</returns>
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