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
         * Return user info claims from a JSON object received in an HTTP response
         */
        public static IEnumerable<Claim> UserInfoClaims(string json)
        {
            var claimsSet = JObject.Parse(json);
            var claims = new List<Claim>();
            claims.Add(ReadClaim(claimsSet, StandardClaimNames.GivenName));
            claims.Add(ReadClaim(claimsSet, StandardClaimNames.FamilyName));
            claims.Add(ReadClaim(claimsSet, StandardClaimNames.Email));
            return claims;
        }

        /*
         * Read a claim from JSON and report missing errors clearly
         */
        private static Claim ReadClaim(JObject claimsSet, string name)
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
