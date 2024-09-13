namespace FinalApi.Logic.Repositories
{
    using FinalApi.Logic.Claims;

    public class UserRepository
    {
        /*
         * Receive the manager ID in the access token, as a useful identity to the API, then look up extra claims
         */
        public SampleExtraClaims GetClaimsForManagerId(string managerId)
        {
            if (managerId == "20116")
            {
                // These claims are used for the guestadmin@example.com user account
                return new SampleExtraClaims("Global Manager", new string[] { "Europe", "USA", "Asia" });
            }
            else if (managerId == "10345")
            {
                // These claims are used for the guestuser@example.com user account
                return new SampleExtraClaims("Regional Manager", new string[] { "USA" });
            }
            else
            {
                // Use empty claims for unrecognized users
                return new SampleExtraClaims(string.Empty, new string[] { });
            }
        }
    }
}
