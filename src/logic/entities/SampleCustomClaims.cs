namespace SampleApi.Logic.Entities
{
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;

    /*
     * Custom claims for our sample API
     */
    public class SampleCustomClaims : CustomClaims
    {
        public SampleCustomClaims(string userDatabaseId, bool isAdmin, string[] regionsCovered)
        {
            this.UserDatabaseId = userDatabaseId;
            this.IsAdmin = isAdmin;
            this.RegionsCovered = regionsCovered;
        }

        public string UserDatabaseId { get; set; }

        public bool IsAdmin { get; set; }

        public string[] RegionsCovered { get; set; }

        public static new SampleCustomClaims ImportData(JObject data)
        {
            var userDatabaseId = data.GetValue("userDatabaseId").Value<string>();
            var isAdmin = data.GetValue("isAdmin").Value<bool>();
            var regionsCovered = data.GetValue("regionsCovered").Value<string>();
            return new SampleCustomClaims(userDatabaseId, isAdmin, regionsCovered.Split(" "));
        }

        public override JObject ExportData()
        {
            dynamic data = new JObject();
            data.userDatabaseId = this.UserDatabaseId;
            data.isAdmin = this.IsAdmin;
            data.regionsCovered = string.Join(" ", this.RegionsCovered);
            return data;
        }
    }
}
