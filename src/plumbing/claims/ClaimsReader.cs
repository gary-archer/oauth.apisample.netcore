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
         * Return user info claims from a JSON object received in an HTTP response
         */
        public static IEnumerable<Claim> UserInfoClaims(string json)
        {
            var claimsSet = JObject.Parse(json);
            var claims = new List<Claim>();
            claims.Add(CheckClaim(claimsSet, OAuthClaimNames.GivenName));
            claims.Add(CheckClaim(claimsSet, OAuthClaimNames.FamilyName));
            claims.Add(CheckClaim(claimsSet, OAuthClaimNames.Email));
            return claims;
        }

        /*
         * Read user info claims from the access token
         */
        public static IEnumerable<Claim> UserInfoClaims(ClaimsModel model)
        {
            var claims = new List<Claim>();
            claims.Add(CheckClaim(OAuthClaimNames.GivenName, model.GivenName));
            claims.Add(CheckClaim(OAuthClaimNames.FamilyName, model.FamilyName));
            claims.Add(CheckClaim(OAuthClaimNames.Email, model.Email));
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
