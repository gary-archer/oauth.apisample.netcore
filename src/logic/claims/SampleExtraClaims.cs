namespace SampleApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Nodes;
    using SampleApi.Plumbing.Claims;

    /*
     * Represents extra claims not received in access tokens
     */
    public class SampleExtraClaims : ExtraClaims
    {
        /*
         * Construct with Claims that are always looked up from the API's own data
         */
        public SampleExtraClaims(string title, string[] regions)
        {
            this.Title = title;
            this.Regions = regions;
            this.ManagerId = null;
            this.Role = null;
        }

        public string Title { get; set; }

        public IEnumerable<string> Regions { get; set; }

        public string ManagerId { get; set; }

        public string Role { get; set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static new SampleExtraClaims ImportData(JsonNode data)
        {
            // These claims are always stored in the cache
            var title = data[CustomClaimNames.Title].GetValue<string>();
            var regionNodes = data[CustomClaimNames.Regions].AsArray();
            var regionsList = new List<string>();
            foreach (var regionNode in regionNodes)
            {
                regionsList.Add(regionNode.GetValue<string>());
            }

            var claims = new SampleExtraClaims(title, regionsList.ToArray());

            // These are only stored in the cache when the authorization server cannot issue them to access tokens
            var managerId = data[CustomClaimNames.ManagerId]?.GetValue<string>();
            var role = data[CustomClaimNames.Role]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(managerId) && !string.IsNullOrWhiteSpace(role))
            {
                claims.AddCoreClaims(managerId, role);
            }

            return claims;
        }

        /*
         * These values should be issued to the access token and store in the JWT claims
         * When not supported by the authorization server they are stored in this class instead
         */
        public void AddCoreClaims(string managerId, string role)
        {
            this.ManagerId = managerId;
            this.Role = role;
        }

        /*
         * Export claims when saving to the cache
         */
        public override JsonNode ExportData()
        {
            // These claims are always stored in the cache
            var data = new JsonObject()
            {
                ["title"] = this.Title,
                ["regions"] = new JsonArray(this.Regions.Select(r => JsonValue.Create(r)).ToArray()),
            };

            // These are only stored in the cache when the authorization server cannot issue them to access tokens
            if (!string.IsNullOrWhiteSpace(this.ManagerId) && !string.IsNullOrWhiteSpace(this.Role))
            {
                data["manager_id"] = this.ManagerId;
                data["role"] = this.Role;
            }

            return data;
        }

        /*
         * Add claims to the identity in order for .NET authorization to work as expected
         */
        public override void AddClaims(ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(CustomClaimNames.Title, this.Title));
            foreach (var region in this.Regions)
            {
                identity.AddClaim(new Claim(CustomClaimNames.Regions, region));
            }

            if (!string.IsNullOrWhiteSpace(this.ManagerId) && !string.IsNullOrWhiteSpace(this.Role))
            {
                identity.AddClaim(new Claim(CustomClaimNames.ManagerId, this.ManagerId));
                identity.AddClaim(new Claim(CustomClaimNames.Role, this.Role));
            }
        }

        /*
         * This claim could be used with .NET attribute based authorization
         */
        public override string GetRoleClaimType()
        {
            return CustomClaimNames.Role;
        }
    }
}
