namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to read claims into objects
     */
    public static class ClaimsReader
    {
        /*
         * Return the base claims in a JWT that the API is interested in
         */
        public static BaseClaims BaseClaims(JObject claimsSet)
        {
            var subject = ClaimsReader.GetClaim(claimsSet, "sub");
            var scopes = ClaimsReader.GetClaim(claimsSet, "scope").Split(' ');
            var expiry = Convert.ToInt32(ClaimsReader.GetClaim(claimsSet, "exp"), CultureInfo.InvariantCulture);
            return new BaseClaims(subject, scopes, expiry);
        }

        /*
         * Return user info claims from a JWT
         */
        public static UserInfoClaims UserInfoClaims(JObject claimsSet)
        {
            var givenName = ClaimsReader.GetClaim(claimsSet, "given_name");
            var familyName = ClaimsReader.GetClaim(claimsSet, "family_name");
            var email = ClaimsReader.GetClaim(claimsSet, "email");
            return new UserInfoClaims(givenName, familyName, email);
        }

        /*
         * Read a claim and report missing errors clearly
         */
        private static string GetClaim(JObject claimsSet, string name)
        {
            var value = claimsSet.GetValue(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value.ToString();
        }
    }
}
