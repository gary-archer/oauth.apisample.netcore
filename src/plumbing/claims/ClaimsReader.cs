namespace SampleApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to read claims into objects
     */
    public static class ClaimsReader
    {
        /*
         * Read base claims from the access token
         */
        public static IEnumerable<Claim> ReadJwtClaims(JwtClaims jwtClaims)
        {
            var claims = new List<Claim>();
            claims.Add(GetClaim(OAuthClaimNames.Subject, jwtClaims.Sub));

            var scopes = jwtClaims.Scope.Split(" ");
            foreach (var scope in scopes)
            {
                claims.Add(GetClaim(OAuthClaimNames.Scope, scope));
            }

            claims.Add(GetClaim(OAuthClaimNames.Exp, jwtClaims.Exp.ToString()));
            return claims;
        }

        /*
         * Return a claim object, checking that it exists first
         */
        private static Claim GetClaim(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return new Claim(name, value);
        }
    }
}
