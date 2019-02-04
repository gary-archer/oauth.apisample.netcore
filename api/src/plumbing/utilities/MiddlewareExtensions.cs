namespace BasicApi.Plumbing.Utilities
{
    using Microsoft.AspNetCore.Builder;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.OAuth;

    /*
     * Simple extension methods
     */
    public static class MiddlewareExtensions
    {
        /*
         * Add custom middleware to improve API error responses
         */
        public static void UseAuthenticationMiddlewareWithErrorHandling(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AuthenticationMiddlewareWithErrorHandling>();
        }

        /*
         * Add custom middleware to manage claims
         */
        public static void UseClaimsMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ClaimsMiddleware>();
        }

        /*
         * Add custom middleware to catch exceptions and return a controlled response
         */
        public static void UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<UnhandledExceptionMiddleware>();
        }
    }
}
