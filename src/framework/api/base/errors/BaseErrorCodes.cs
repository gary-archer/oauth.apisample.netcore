namespace Framework.Api.Base.Errors
{
    /*
     * Error codes owned by the base API framework
     */
    public static class BaseErrorCodes
    {
        public const string UnauthorizedRequest = "unauthorized";

        public const string ServerError = "server_error";

        public const string ClaimsFailure = "claims_failure";

        public const string ExceptionSimulation = "exception_simulation";
    }
}
