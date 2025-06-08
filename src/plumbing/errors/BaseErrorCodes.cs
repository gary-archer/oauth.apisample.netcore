namespace FinalApi.Plumbing.Errors
{
    /*
     * Error codes used by plumbing classes
     */
    public static class BaseErrorCodes
    {
        public static readonly string ServerError = "server_error";

        public static readonly string InvalidToken = "invalid_token";

        public static readonly string TokenSigningKeysDownloadError = "jwks_download_failure";

        public static readonly string InsufficientScope = "insufficient_scope";

        public static readonly string ExceptionSimulation = "exception_simulation";
    }
}
