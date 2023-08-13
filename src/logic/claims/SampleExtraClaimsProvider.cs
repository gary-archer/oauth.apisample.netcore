namespace SampleApi.Logic.Claims
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public class SampleExtraClaimsProvider : ExtraClaimsProvider
    {
        /*
         * This is called to get extra claims when the token is first received
         */
        #pragma warning disable 1998
        public override async Task<ExtraClaims> LookupBusinessClaimsAsync(string accessToken, JwtClaims jwtClaims)
        {
            // It is common to need to get a business user ID for the authenticated user
            // In our example a manager user may be able to view information about investors
            var managerId = this.GetManagerId(jwtClaims);

            // A real API would use a database, but this API uses a mock implementation
            if (managerId == "20116")
            {
                // These claims are used for the guestadmin@mycompany.com user account
                return new SampleExtraClaims(managerId, "admin", new string[] { "Europe", "USA", "Asia" });
            }
            else
            {
                // These claims are used for the guestuser@mycompany.com user account
                return new SampleExtraClaims(managerId, "user", new string[] { "USA" });
            }
        }
        #pragma warning restore 1998

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public override ExtraClaims DeserializeFromCache(JObject data)
        {
            return SampleExtraClaims.ImportData(data);
        }

        /*
         * Get a business user ID that corresponds to the user in the token
         */
        private string GetManagerId(JwtClaims jwtClaims)
        {
            var managerId = jwtClaims.GetOptionalClaim(CustomClaimNames.ManagerId);
            if (string.IsNullOrWhiteSpace(managerId))
            {
                // The preferred option is for the API to receive the business user identity in the JWT access token
                return managerId;
            }
            else
            {
                // Otherwise the API must determine the value from the subject claim
                return this.LookupManagerIdFromSubjectClaim(jwtClaims.Sub);
            }
        }

        /*
         * The API could store a mapping from the subject claim to the business user identity
         */
        private string LookupManagerIdFromSubjectClaim(string subject)
        {
            // A real API would use a database, but this API uses a mock implementation
            // This subject value is for the guestadmin@mycompany.com user account
            var isAdmin = subject == "77a97e5b-b748-45e5-bb6f-658e85b2df91";
            if (isAdmin)
            {
                return "20116";
            }
            else
            {
                return "10345";
            }
        }
    }
}
