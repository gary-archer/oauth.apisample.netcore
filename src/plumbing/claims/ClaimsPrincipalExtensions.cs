namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using SampleApi.Plumbing.Errors;

    /*
     * Convenience extensions to return values once there is a claims principal
     */
    public static class ClaimsPrincipalExtensions
    {
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            return principal.ReadStringClaim(OAuthClaimNames.Subject).Value;
        }

        public static IEnumerable<string> GetScopes(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type == OAuthClaimNames.Scope).Select(s => s.Value);
        }

        public static int GetExpiry(this ClaimsPrincipal principal)
        {
            var expValue = principal.ReadStringClaim(OAuthClaimNames.Exp).Value;
            return Convert.ToInt32(expValue, CultureInfo.InvariantCulture);
        }

        public static Claim ReadStringClaim(this ClaimsPrincipal principal, string name)
        {
            var found = principal.Claims.FirstOrDefault(c => c.Type == name);
            if (found == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return found;
        }
    }
}
