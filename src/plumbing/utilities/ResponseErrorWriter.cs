namespace FinalApi.Plumbing.Utilities
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FinalApi.Plumbing.Errors;
    using Microsoft.AspNetCore.Http;

    /*
     * A helper methods to write error responses
     */
    public static class ResponseErrorWriter
    {
        /*
         * This blog's clients read a JSON response, to handle OAuth errors in the same way as other errors
         * Also add the standard www-authenticate header for interoperability
         */
        public static async Task WriteErrorResponse(HttpResponse response, ClientError error, string scope)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)error.StatusCode;

            if (error.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WWWAuthenticate =
                    $"Bearer error=\"{error.ErrorCode}\", error_description=\"{error.Message}\"";
            }

            if (error.StatusCode == HttpStatusCode.Forbidden)
            {
                response.Headers.WWWAuthenticate =
                    $"Bearer error=\"{error.ErrorCode}\", error_description=\"{error.Message}\", scope=\"{scope}\"";
            }

            await response.WriteAsync(error.ToResponseFormat().ToString());
        }
    }
}
