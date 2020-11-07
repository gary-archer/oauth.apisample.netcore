namespace SampleApi.Plumbing.Errors
{
    /*
     * Error codes that could be used by multiple APIs
     */
    public static class ErrorCodes
    {
        public const string ServerError = "server_error";

        public const string UnauthorizedRequest = "unauthorized";

        public const string ClaimsFailure = "claims_failure";

        public const string MetadataLookupFailure = "metadata_lookup_failure";

        public const string IntrospectionFailure = "introspection_failure";

        public const string TokenSigningKeysDownloadError = "jwks_download_failure";

        public const string UserInfoFailure = "userinfo_failure";

        public const string UserInfoTokenExpired = "invalid_token";

        public const string ExceptionSimulation = "exception_simulation";
    }
}
