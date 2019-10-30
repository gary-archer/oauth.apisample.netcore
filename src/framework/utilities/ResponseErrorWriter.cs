namespace Framework.Utilities
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Framework.Errors;

    /*
     * A helper class to write response errors
     */
    public static class ResponseErrorWriter
    {
        /*
         * Deliver a controlled 401 response to the caller
         */
        public static async Task WriteInvalidTokenResponse(HttpRequest request, HttpResponse response)
        {
            // Write headers
            response.ContentType = "application/json";
            response.Headers.Add("WWW-Authenticate", "Bearer");
            AddCorsHeaderForErrorResponse(request, response);
            
            // Write the body
            response.StatusCode = 401;
            var clientError = new ClientError(HttpStatusCode.Unauthorized, "unauthorized", "Missing, invalid or expired access token");
            await response.WriteAsync(clientError.ToResponseFormat().ToString());
        }

        /*
         * Deliver a controlled 500 response to the caller
         */
        public static async Task WriteErrorResponse(HttpRequest request, HttpResponse response, int statusCode, JObject error)
        {
            // Write headers
            response.ContentType = "application/json";
            AddCorsHeaderForErrorResponse(request, response);
            
            // Write the body
            response.StatusCode = statusCode;
            await response.WriteAsync(error.ToString());
        }

        /*
         * The CORS response header is not always written for error responses
         * So we write it here so that browser clients can read the error response as JSON
         */
        private static void AddCorsHeaderForErrorResponse(HttpRequest request, HttpResponse response)
        {
            const string CORS_HEADER = "Access-Control-Allow-Origin";

            if (response.Headers[CORS_HEADER].Count == 0)
            {
                var originHeader = request.Headers["Origin"];	
                if (originHeader.Count > 0)	
                {	
                    response.Headers.Add(CORS_HEADER, originHeader.ToString());	
                }
            }
        }
    }
}
