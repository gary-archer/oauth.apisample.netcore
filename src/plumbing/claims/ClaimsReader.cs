namespace SampleApi.Plumbing.Claims
{
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
            JToken claim = GetOptionalClaim(claims, name).Value<string>();
            if (claim == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return claim.Value<string>();
        }

        /*
         * Return an optional string claim
         */
        public static string GetOptionalStringClaim(JObject claims, string name)
        {
            var claim = claims.GetValue(name);
            return claim?.Value<string>();
        }

        /*
         * Get a claim of any type, checking that it exists first
         */
        public static JToken GetOptionalClaim(JObject claims, string name)
        {
            return claims.GetValue(name);
        }
    }
}
