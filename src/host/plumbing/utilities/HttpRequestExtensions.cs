namespace SampleApi.Host.Plumbing.Utilities
{
    using Microsoft.AspNetCore.Http;

    /*
     * Helper methods to deal with requests
     */
    public static class HttpRequestExtensions
    {
        /*
         * Return a header or null
         */
        public static string GetHeader(this HttpRequest request, string name)
        {
            if (request.Headers != null && request.Headers.ContainsKey(name))
            {
                return request.Headers[name];
            }

            return null;
        }
    }
}