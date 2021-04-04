namespace SampleApi.Logic.Entities
{
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;

    /*
     * Custom claims for our sample API
     */
    public class SampleCustomClaims : CustomClaims
    {
        public SampleCustomClaims(string userId, string userRole, string[] userRegions)
        {
            this.UserId = userId;
            this.UserRole = userRole;
            this.UserRegions = userRegions;
        }

        public string UserId { get; set; }

        public string UserRole { get; set; }

        public string[] UserRegions { get; set; }

        public static new SampleCustomClaims ImportData(JObject data)
        {
            var userId = data.GetValue("userId").Value<string>();
            var userRole = data.GetValue("userRole").Value<string>();

            var userRegionsNode = data.GetValue("userRegions").ToArray();
            var userRegions = userRegionsNode.Select(node => node.Value<string>());

            return new SampleCustomClaims(userId, userRole, userRegions.ToArray());
        }

        public override JObject ExportData()
        {
            dynamic data = new JObject();
            data.userId = this.UserId;
            data.userRole = this.UserRole;

            data.userRegions = new JArray();
            foreach (var region in this.UserRegions)
            {
                data.userRegions.Add(region);
            }

            return data;
        }
    }
}
