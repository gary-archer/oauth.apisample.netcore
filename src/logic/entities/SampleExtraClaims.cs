namespace SampleApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;

    /*
     * Some example claims that may not be present in the access token
     * In some cases this may be due to authorization server limitations
     * In other cases they may be easier to manage outside the authorization server
     * The API's service logic treats such values as claims though
     */
    public class SampleExtraClaims : ExtraClaims
    {
        public SampleExtraClaims(string managerId, string role, string[] regions)
        {
            this.ManagerId = managerId;
            this.Role = role;
            this.Regions = regions.AsEnumerable();
        }

        public string ManagerId { get; set; }

        public string Role { get; set; }

        public IEnumerable<string> Regions { get; set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static new SampleExtraClaims ImportData(JObject data)
        {
            var managerId = data.GetValue(CustomClaimNames.ManagerId).Value<string>();
            var role = data.GetValue(CustomClaimNames.Role).Value<string>();
            var regionNodes = data.GetValue(CustomClaimNames.Regions).Value<JArray>();

            var regionsList = new List<string>();
            foreach (var regionNode in regionNodes)
            {
                regionsList.Add(regionNode.Value<string>());
            }

            return new SampleExtraClaims(managerId, role, regionsList.ToArray());
        }

        /*
         * Export claims when saving to the cache
         */
        public override JObject ExportData()
        {
            dynamic data = new JObject();
            data.manager_id = this.ManagerId;
            data.role = this.Role;

            var regions = new JArray();
            foreach (var region in this.Regions)
            {
                regions.Add(region);
            }

            data.regions = regions;
            return data;
        }

        /*
         * Return all claims as a list, to be added to the ClaimsPrincipal
         * This ensures that .NET authorization attributes work as expected
         */
        public override IEnumerable<Claim> ToList()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(CustomClaimNames.ManagerId, this.ManagerId));
            claims.Add(new Claim(CustomClaimNames.Role, this.Role));

            foreach (var region in this.Regions)
            {
                claims.Add(new Claim(CustomClaimNames.Regions, region));
            }

            return claims;
        }
    }
}
