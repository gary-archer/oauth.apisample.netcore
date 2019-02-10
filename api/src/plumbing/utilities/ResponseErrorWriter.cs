namespace BasicApi.Plumbing.Utilities
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using BasicApi.Entities;
    using Newtonsoft.Json.Serialization;

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
            response.Headers.Add("WWW-Authenticate", "Bearer");
            AddCorsHeaderForErrorResponse(request, response);
            
            // Write the body
            response.StatusCode = 401;
            response.ContentType = "application/json";
            var jsonData = JsonConvert.SerializeObject("Missing, invalid or expired access token");
            await response.WriteAsync(jsonData);
        }

        /*
         * Deliver a controlled 500 response to the caller
         */
        public static async Task WriteErrorResponse(HttpRequest request, HttpResponse response, int statusCode, object error)
        {
            // Write headers
            response.ContentType = "application/json";
            AddCorsHeaderForErrorResponse(request, response);
            
            // Write the body
            response.StatusCode = statusCode;
            await response.WriteAsync(error.ToString());
        }

        /* 
         * Annoyingly the CORS response header is not always written for error responses
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
