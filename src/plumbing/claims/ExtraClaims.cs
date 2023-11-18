namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;
    using System.Text.Json.Nodes;

    /*
     * Claims that you cannot, or do not want to, manage in the authorization server
     */
    public class ExtraClaims
    {
        /*
         * Overridden by derived classes when reading claims from the cache
         */
        public static ExtraClaims ImportData(JsonNode data)
        {
            return new ExtraClaims();
        }

        /*
         * Overridden by derived classes when saving claims to the cache
         */
        public virtual JsonNode ExportData()
        {
            return new JsonObject();
        }

        /*
         * Add claims to the identity in order for .NET authorization to work as expected
         */
        public virtual void AddClaims(ClaimsIdentity identity)
        {
        }

        /*
         * A derived class can set the role claim type
         */
        public virtual string GetRoleClaimType()
        {
            return null;
        }
    }
}
