namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;
    using SampleApi.Plumbing.Errors;

    /*
     * Utility methods when working with a claims principal
     */
    public static class ClaimsPrincipalExtensions
    {
        /*
         * Get a claim and clearly report missing data
         */
        public static string GetClaim(this ClaimsPrincipal principal, string name)
        {
            var claim = principal.FindFirst(c => c.Type == name);
            if (claim == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return claim.Value;
        }
    }
}
