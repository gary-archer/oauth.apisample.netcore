namespace SampleApi.Plumbing.Utilities
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * A helper class to write response errors
     */
    public static class ResponseErrorWriter
    {
        /*
         * Deliver a controlled 401 response to the caller
         */
        public static async Task WriteInvalidTokenResponse(HttpResponse response, ClientError error)
        {
            // Write headers
            response.ContentType = "application/json";
            response.Headers.Add("WWW-Authenticate", "Bearer");
            response.StatusCode = (int)error.StatusCode;
            await response.WriteAsync(error.ToResponseFormat().ToString());
        }

        /*
         * Deliver a controlled 500 response to the caller
         */
        public static async Task WriteErrorResponse(HttpResponse response, HttpStatusCode statusCode, JObject error)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;
            await response.WriteAsync(error.ToString());
        }
    }
}
