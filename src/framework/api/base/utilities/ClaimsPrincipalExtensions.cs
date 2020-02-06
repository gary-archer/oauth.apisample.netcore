namespace Framework.Api.Base.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /*
     * Helper methods to deal with claims set against the claims principal
     */
    public static class ClaimsPrincipalExtensions
    {
        /*
         * Get a string claim from the claims principal and check results
         */
        public static string GetStringClaim(this ClaimsPrincipal principal, string name)
        {
            string value = principal.Claims.FirstOrDefault(c => c.Type == name)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Unable to read claim {name} from the claims principal");
            }

            return value;
        }

        /*
         * Get a set of string claims from the claims principal and check results
         */
        public static IEnumerable<string> GetStringClaimSet(this ClaimsPrincipal principal, string name)
        {
            var stringValues = principal.Claims.Where(c => c.Type == name).Select(c => c.Value).ToList();
            if (stringValues.Count == 0)
            {
                throw new InvalidOperationException($"Unable to read collection claim {name} from the claims principal");
            }

            return stringValues;
        }
    }
}