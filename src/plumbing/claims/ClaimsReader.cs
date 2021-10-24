namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Globalization;
    using IdentityModel;
    using IdentityModel.Client;
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
            var subject = ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.Subject);
            var scopes = ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.Scope).Split(' ');
            var expiry = Convert.ToInt32(ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);
            return new BaseClaims(subject, scopes, expiry);
        }

        /*
         * Return user info claims from a JWT
         */
        public static UserInfoClaims UserInfoClaims(JObject claimsSet)
        {
            var givenName = ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.GivenName);
            var familyName = ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.FamilyName);
            var email = ClaimsReader.GetClaim(claimsSet, JwtClaimTypes.Email);
            return new UserInfoClaims(givenName, familyName, email);
        }

        /*
         * Return the user info claims from a JWT
         */
        public static UserInfoClaims UserInfoClaims(UserInfoResponse response)
        {
            var givenName = ClaimsReader.GetClaim(response, JwtClaimTypes.GivenName);
            var familyName = ClaimsReader.GetClaim(response, JwtClaimTypes.FamilyName);
            var email = ClaimsReader.GetClaim(response, JwtClaimTypes.Email);
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

        /*
         * Read a claim and report missing errors clearly
         */
        private static string GetClaim(UserInfoResponse response, string name)
        {
            var value = response.TryGet(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value.ToString();
        }
    }
}
