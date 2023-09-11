namespace SampleApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;
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
        public static new SampleExtraClaims ImportData(JObject data)
        {
            // These claims are always stored in the cache
            var title = data.GetValue(CustomClaimNames.Title).Value<string>();
            var regionNodes = data.GetValue(CustomClaimNames.Regions).Value<JArray>();
            var regionsList = new List<string>();
            foreach (var regionNode in regionNodes)
            {
                regionsList.Add(regionNode.Value<string>());
            }

            var claims = new SampleExtraClaims(title, regionsList.ToArray());

            // These are only stored in the cache when the authorization server cannot issue them to access tokens
            var managerIdNode = data.GetValue(CustomClaimNames.ManagerId);
            var roleNode = data.GetValue(CustomClaimNames.Role);
            if (managerIdNode != null && roleNode != null)
            {
                claims.AddCoreClaims(managerIdNode.Value<string>(), roleNode.Value<string>());
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
        public override JObject ExportData()
        {
            dynamic data = new JObject();

            // These claims are always stored in the cache
            data.title = this.Title;
            var regions = new JArray();
            foreach (var region in this.Regions)
            {
                regions.Add(region);
            }

            data.regions = regions;

            // These are only stored in the cache when the authorization server cannot issue them to access tokens
            if (!string.IsNullOrWhiteSpace(this.ManagerId) && !string.IsNullOrWhiteSpace(this.Role))
            {
                data.manager_id = this.ManagerId;
                data.role = this.Role;
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
