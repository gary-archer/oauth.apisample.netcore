namespace FinalApi.Plumbing.Claims
{
    using System.Text.Json.Nodes;
    using FinalApi.Plumbing.Errors;

    /*
     * A simple utility class to read claim values safely
     */
    public static class ClaimsReader
    {
        /*
         * Return a mandatory string claim
         */
        public static string GetStringClaim(JsonNode claims, string name)
        {
            var claim = claims[name]?.GetValue<string>();
            if (claim == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return claim;
        }

        /*
         * Return a mandatory integer claim
         */
        public static int GetIntegerClaim(JsonNode claims, string name)
        {
            var claim = claims[name]?.GetValue<int>();
            if (claim == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return claim.Value;
        }
    }
}
