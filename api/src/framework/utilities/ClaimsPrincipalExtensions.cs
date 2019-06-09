namespace Framework.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Helper methods to deal with claims set against the claims principal
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Get a string claim from the claims principal and check results
        /// </summary>
        /// <param name="principal">The claims principal</param>
        /// <param name="name">The name of the claim</param>
        /// <returns>The claim value</returns>
        public static string GetStringClaim(this ClaimsPrincipal principal, string name)
        {
            string value = principal.Claims.FirstOrDefault(c => c.Type == name)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Unable to read claim {name} from the claims principal");
            }

            return value;
        }

        /// <summary>
        /// Get a set of string claims from the claims principal and check results
        /// </summary>
        /// <param name="principal">The claims principal</param>
        /// <param name="name">The name of the claim set</param>
        /// <returns>The set of claim values</returns>
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