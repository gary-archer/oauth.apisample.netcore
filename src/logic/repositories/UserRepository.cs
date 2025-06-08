namespace FinalApi.Logic.Repositories
{
    using FinalApi.Plumbing.Claims;

    /*
     * A repository that returns hard coded data, whereas a real implementation would use a database lookup
     */
    public class UserRepository
    {
        /*
         * Receive the manager ID in the access token, as a useful API identity, then look up extra authorization values
         */
        public ExtraClaims GetUserInfoForManagerId(string managerId)
        {
            if (managerId == "20116")
            {
                // These values are used for the guestadmin@example.com user account
                return new ExtraClaims(
                    "Global Manager",
                    ["Europe", "USA", "Asia"]);
            }
            else if (managerId == "10345")
            {
                // These values are used for the guestuser@example.com user account
                return new ExtraClaims(
                    "Regional Manager",
                    ["USA"]);
            }
            else
            {
                // Use empty values for unrecognized users
                return new ExtraClaims();
            }
        }
    }
}
