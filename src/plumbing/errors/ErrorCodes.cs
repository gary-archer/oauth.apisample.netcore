namespace SampleApi.Plumbing.Errors
{
    /*
     * Error codes that could be used by multiple APIs
     */
    public static class ErrorCodes
    {
        public static readonly string ServerError = "server_error";

        public static readonly string InvalidToken = "invalid_token";

        public static readonly string TokenSigningKeysDownloadError = "jwks_download_failure";

        public static readonly string InsufficientScope = "insufficient_scope";

        public static readonly string ExceptionSimulation = "exception_simulation";
    }
}
