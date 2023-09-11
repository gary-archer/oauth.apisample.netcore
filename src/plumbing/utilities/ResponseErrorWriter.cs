namespace SampleApi.Plumbing.Utilities
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Errors;

    /*
     * Return an error response from the API to the client
     */
    public static class ResponseErrorWriter
    {
        /*
         * Deliver a controlled 500 response to the caller
         */
        public static async Task WriteErrorResponse(HttpResponse response, ClientError error)
        {
            // Add the standards based header if required
            if (error.StatusCode == HttpStatusCode.Unauthorized)
            {
                var realm = "mycompany.com";
                var wwwAuthenticateHeader = $"Bearer realm=\"{realm}\", error=\"{error.ErrorCode}\", error_description=\"{error.Message}\"";
                response.Headers["www-authenticate"] = wwwAuthenticateHeader;
            }

            // Also add a more client friendly JSON response with the same fields
            response.ContentType = "application/json";
            response.StatusCode = (int)error.StatusCode;
            await response.WriteAsync(error.ToResponseFormat().ToString());
        }
    }
}
