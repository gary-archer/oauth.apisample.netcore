namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;

    /*
     * Extensions to claims principal behaviour
     */
    public static class ClaimsPrincipalExtensions
    {
        /*
         * Extend the claims in the JWT with those retrived from other sources
         */
        public static ClaimsPrincipal ExtendClaims(this ClaimsPrincipal principal, IEnumerable<Claim> extraClaims)
        {
            // Include both token claims and those retrieved from external sources
            var claims = new List<Claim>();
            claims.AddRange(principal.Claims);
            claims.AddRange(extraClaims);

            // All claims are available to use for authorization
            var identity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType);
            return new ClaimsPrincipal(identity);
        }

        /*
         * Return custom claims, which do not have an issuer
         */
        public static IEnumerable<Claim> GetCustomClaims(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => string.IsNullOrWhiteSpace(c.Issuer));
        }

        /*
         * Convenience accessors
         */
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, StandardClaimNames.Subject).Value;
        }

        public static string[] GetScopes(this ClaimsPrincipal principal)
        {
            var scopeValue = ClaimsReader.ReadClaim(principal, StandardClaimNames.Scope).Value;
            return scopeValue.Split(' ');
        }

        public static int GetExpiry(this ClaimsPrincipal principal)
        {
            var expValue = ClaimsReader.ReadClaim(principal, StandardClaimNames.Exp).Value;
            return Convert.ToInt32(expValue, CultureInfo.InvariantCulture);
        }

        public static string GetGivenName(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, StandardClaimNames.GivenName).Value;
        }

        public static string GetFamilyName(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, StandardClaimNames.FamilyName).Value;
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, StandardClaimNames.Email).Value;
        }
    }
}
