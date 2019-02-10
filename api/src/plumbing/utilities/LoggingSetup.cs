namespace BasicApi.Plumbing.Utilities
{
    using Microsoft.Extensions.Logging;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.OAuth;

    /*
     * Encapsulate logging logic here
     */
    public static class LoggingSetup
    {
        /*
         * Reduce output to only sources we care about
         */
        public static bool Filter(string category, LogLevel level)
        {
            switch (category)
            {
                case var a when a == typeof(AuthenticationMiddlewareWithErrorHandling).FullName:
                case var b when b == typeof(ClaimsMiddleware).FullName:
                case var c when c == typeof(UnhandledExceptionMiddleware).FullName:
                    return true;

                default:
                    return false;
            }
        }
    }
}