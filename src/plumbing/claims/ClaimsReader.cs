namespace SampleApi.Plumbing.Claims
{
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * A simple utility class to read claim values safely
     */
    public static class ClaimsReader
    {
        /*
         * Return a mandatory string claim
         */
        public static string GetStringClaim(JObject claims, string name)
        {
            return GetClaim(claims, name).Value<string>();
        }

        /*
         * Return an optional string claim
         */
        public static JToken GetOptionalStringClaim(JObject claims, string name)
        {
            return claims.GetValue(name);
        }

        /*
         * Get a claim of any type, checking that it exists first
         */
        public static JToken GetClaim(JObject claims, string name)
        {
            var claim = claims.GetValue(name);
            if (claim == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return claim.Value<string>();
        }
    }
}
