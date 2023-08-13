namespace SampleApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to read claims into objects
     */
    public static class ClaimsReader
    {
        /*
         * Read base claims from the access token
         */
        public static IEnumerable<Claim> BaseClaims(ClaimsModel model)
        {
            var claims = new List<Claim>();
            claims.Add(CheckClaim(OAuthClaimNames.Subject, model.Sub));

            var scopes = model.Scope.Split(" ");
            foreach (var scope in scopes)
            {
                claims.Add(CheckClaim(OAuthClaimNames.Scope, scope));
            }

            claims.Add(CheckClaim(OAuthClaimNames.Exp, model.Exp.ToString()));
            return claims;
        }

        /*
         * Return a claim object, checking that it exists first
         */
        private static Claim CheckClaim(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return new Claim(name, value);
        }

        /*
         * Read a claim from JSON and report missing claims clearly
         */
        private static Claim CheckClaim(JObject claimsSet, string name)
        {
            var value = claimsSet.GetValue(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return new Claim(name, value.ToString());
        }
    }
}
