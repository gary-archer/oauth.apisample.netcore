namespace SampleApi.Host.Claims
{
    using System.Threading.Tasks;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * A custom claims provider to include extra domain specific claims in the claims cache
     */
    public class SampleCustomClaimsProvider : CustomClaimsProvider
    {
        /*
         * An example of how custom claims can be included
         */
        public override Task<CustomClaims> GetCustomClaimsAsync(TokenClaims token, UserInfoClaims userInfo)
        {
            // A real implementation would look up the database user id from the subject and / or email claim
            var email = userInfo.Email;
            var userDatabaseId = "10345";

            // Our blog's code samples have two fixed users and we use the below mock implementation:
            // - guestadmin@mycompany.com is an admin and sees all data
            // - guestuser@mycompany.com is not an admin and only sees data for the USA region
            var isAdmin = userInfo.Email.ToLower().Contains("admin");
            var regionsCovered = isAdmin ? new string[] { } : new[] { "USA" };

            CustomClaims claims = new SampleCustomClaims(userDatabaseId, isAdmin, regionsCovered);
            return Task.FromResult(claims);
        }
    }
}