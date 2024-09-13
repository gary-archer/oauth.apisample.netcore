namespace FinalApi.Plumbing.OAuth
{
    using Microsoft.AspNetCore.Http;
    using FinalApi.Plumbing.Utilities;

    /*
     * A simple utility class to read the access token
     */
    internal static class BearerToken
    {
        /*
         * Read the access token from the Authorization header
         */
        public static string Read(HttpRequest request)
        {
            string authorization = request.GetHeader("Authorization");
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0].ToLowerInvariant() == "bearer")
                {
                    return parts[1];
                }
            }

            return null;
        }
    }
}
