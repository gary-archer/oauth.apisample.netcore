namespace SampleApi.Plumbing.OAuth
{
    using System.Linq;
    using System.Net;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to verify scopes
     */
    public static class ScopeVerifier
    {
        public static void Enforce(string[] scopes, string requiredScope)
        {
            var found = scopes.FirstOrDefault(s => s.Contains(requiredScope));
            if (found == null)
            {
                throw ErrorFactory.CreateClientError(
                    HttpStatusCode.Forbidden,
                    ErrorCodes.InsufficientScope,
                    "Access token does not have a valid scope for this API endpoint");
            }
        }
    }
}
