namespace SampleApi.Plumbing.OAuth
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    /*
     * A simple utility class to read the access token
     */
    internal static class BearerToken
    {
        /*
         * OAuth authorization involves token validation and claims lookup
         */
        public static string Read(HttpRequest request)
        {
            string authorization = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0] == "Bearer")
                {
                    return parts[1];
                }
            }

            return null;
        }
    }
}
