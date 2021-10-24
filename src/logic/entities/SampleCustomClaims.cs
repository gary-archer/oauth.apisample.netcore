namespace SampleApi.Logic.Entities
{
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;

    /*
     * Custom claims for our sample API
     */
    public class SampleCustomClaims : CustomClaims
    {
        /*
         * Used with the claims caching authorizer
         */
        public SampleCustomClaims(string userId, string userRole, string[] userRegions)
        {
            this.UserId = userId;
            this.UserRole = userRole;
            this.UserRegions = userRegions;
        }

        public string UserId { get; set; }

        public string UserRole { get; set; }

        public string[] UserRegions { get; set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static new SampleCustomClaims ImportData(JObject data)
        {
            var userId = data.GetValue("user_id").Value<string>();
            var userRole = data.GetValue("user_role").Value<string>();
            var userRegions = data.GetValue("user_regions").ToArray().Select(node => node.Value<string>());

            return new SampleCustomClaims(userId, userRole, userRegions.ToArray());
        }

        /*
         * Called when claims are serialized during claims caching
         */
        public override JObject ExportData()
        {
            dynamic data = new JObject();
            data.user_id = this.UserId;
            data.user_role = this.UserRole;

            data.user_regions = new JArray();
            foreach (var region in this.UserRegions)
            {
                data.user_regions.Add(region);
            }

            return data;
        }
    }
}
