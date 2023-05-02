namespace SampleApi.Plumbing.OAuth
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to verify scopes
     */
    public static class ScopeVerifier
    {
        /*
         * Deny access unless a required scope is present
         */
        public static void Enforce(IEnumerable<string> scopes, string requiredScope)
        {
            var found = scopes.FirstOrDefault(s => s.Contains(requiredScope));
            if (found == null)
            {
                Deny();
            }
        }

        /*
         * Return the 403 forbidden error
         */
        public static void Deny()
        {
            throw ErrorFactory.CreateClientError(
                    HttpStatusCode.Forbidden,
                    ErrorCodes.InsufficientScope,
                    "Access to this API endpoint is forbidden");
        }
    }
}
