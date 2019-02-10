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
        public static async Task WriteInvalidTokenResponse(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", "Bearer");

            AddCorsHeaderForErrorResponse(context);
            context.Response.ContentType = "application/json";
            var jsonData = JsonConvert.SerializeObject("Missing, invalid or expired access token");
            await context.Response.WriteAsync(jsonData);
        }

        /*
         * Deliver a controlled 500 response to the caller
         */
        public static async Task WriteErrorResponse(HttpContext context, int statusCode, object error)
        {
            AddCorsHeaderForErrorResponse(context);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(error.ToString());
        }

        /* 
         * Annoyingly the CORS response header is not always written for error responses
         * So we write it here so that browser clients can read the error response as JSON
         */
        private static void AddCorsHeaderForErrorResponse(HttpContext context)
        {
            const string CORS_HEADER = "Access-Control-Allow-Origin";

            if (context.Response.Headers[CORS_HEADER].Count == 0)
            {
                var originHeader = context.Request.Headers["Origin"];	
                if (originHeader.Count > 0)	
                {	
                    context.Response.Headers.Add(CORS_HEADER, originHeader.ToString());	
                }
            }
        }
    }
}
