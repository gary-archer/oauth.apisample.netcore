namespace SampleApi.Logic.Entities
{
    using SampleApi.Plumbing.Claims;

    /*
     * Custom claims for our sample API
     */
    public class SampleCustomClaims: CustomClaims
    {
        public string UserDatabaseId { get; set; }

        public bool IsAdmin { get; set; }

        public string[] RegionsCovered { get; set; }


        public SampleCustomClaims(string userDatabaseId, bool isAdmin, string[] regionsCovered)
        {
            this.UserDatabaseId = userDatabaseId;
            this.IsAdmin = isAdmin;
            this.RegionsCovered = regionsCovered;
        }
    }
}
