namespace FinalApi.Plumbing.Utilities
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using FinalApi.Plumbing.Errors;

    /*
     * Return secure errors to clients
     */
    public static class ResponseErrorWriter
    {
        /*
         * This blog's examples use a JSON response to provide client friendly OAuth errors
         * When required, such as to inform clients how to integrate, a www-authenticate header can be added here
         * - https://datatracker.ietf.org/doc/html/rfc6750#section-3
         */
        public static async Task WriteErrorResponse(HttpResponse response, ClientError error)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)error.StatusCode;
            await response.WriteAsync(error.ToResponseFormat().ToString());
        }
    }
}
