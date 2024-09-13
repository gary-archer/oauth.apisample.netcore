namespace FinalApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Nodes;
    using FinalApi.Plumbing.Claims;

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
        }

        public string Title { get; set; }

        public IEnumerable<string> Regions { get; set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static new SampleExtraClaims ImportData(JsonNode data)
        {
            var title = data[CustomClaimNames.Title].GetValue<string>();
            var regionNodes = data[CustomClaimNames.Regions].AsArray();
            var regionsList = new List<string>();
            foreach (var regionNode in regionNodes)
            {
                regionsList.Add(regionNode.GetValue<string>());
            }

            return new SampleExtraClaims(title, regionsList.ToArray());
        }

        /*
         * Export claims when saving to the cache
         */
        public override JsonNode ExportData()
        {
            return new JsonObject()
            {
                ["title"] = this.Title,
                ["regions"] = new JsonArray(this.Regions.Select(r => JsonValue.Create(r)).ToArray()),
            };
        }

        /*
         * Add claims to the identity in order for .NET authorization to work as expected
         */
        public override void AddToClaimsIdentity(ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(CustomClaimNames.Title, this.Title));
            foreach (var region in this.Regions)
            {
                identity.AddClaim(new Claim(CustomClaimNames.Regions, region));
            }
        }
    }
}
