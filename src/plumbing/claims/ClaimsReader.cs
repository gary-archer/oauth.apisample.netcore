namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Globalization;
    using System.Security.Claims;
    using IdentityModel;

    /*
     * A simple utility class to read claims into objects
     */
    public static class ClaimsReader
    {
        /*
         * Return the base claims in a JWT that the API is interested in
         */
        public static BaseClaims BaseClaims(ClaimsPrincipal claimsPrincipal)
        {
            var subject = claimsPrincipal.GetClaim(JwtClaimTypes.Subject);
            var scopes = claimsPrincipal.GetClaim(JwtClaimTypes.Scope).Split(' ');
            var expiry = Convert.ToInt32(claimsPrincipal.GetClaim(JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);
            return new BaseClaims(subject, scopes, expiry);
        }

        /*
         * Return the user info claims from a JWT
         */
        public static UserInfoClaims UserInfoClaims(ClaimsPrincipal claimsPrincipal)
        {
            var givenName = claimsPrincipal.GetClaim(JwtClaimTypes.GivenName);
            var familyName = claimsPrincipal.GetClaim(JwtClaimTypes.FamilyName);
            var email = claimsPrincipal.GetClaim(JwtClaimTypes.Email);
            return new UserInfoClaims(givenName, familyName, email);
        }
    }
}
