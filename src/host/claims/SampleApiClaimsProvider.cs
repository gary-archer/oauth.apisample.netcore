namespace SampleApi.Host.Authorization
{
    using System.Threading.Tasks;
    using SampleApi.Host.Claims;
    using SampleApi.Plumbing.Claims;

    /*
     * A custom claims provider to include extra domain specific claims in the claims cache
     */
    public class SampleApiClaimsProvider : CustomClaimsProvider<SampleApiClaims>
    {
        /*
         * The interface supports returning results based on the user id from the token
         * This might involve a database lookup or a call to another service
         */
        public override Task AddCustomClaimsAsync(string accessToken, SampleApiClaims claims)
        {
            // Look up the user id in the API's own database
            this.LookupDatabaseUserId(claims);

            // Look up the user id in the API's own data
            this.LookupAuthorizationData(claims);
            return Task.FromResult(0);
        }

        /*
        * A real implementation would get the subject / email claims and find a match in the API's own data
        */
        private void LookupDatabaseUserId(SampleApiClaims claims)
        {
            claims.UserDatabaseId = "10345";
        }

        /*
         * A real implementation would look up authorization data from the API's own data
         */
        private void LookupAuthorizationData(SampleApiClaims claims)
        {
            // Our blog's code samples have two fixed users and use the below mock implementation:
            // - guestadmin@mycompany.com is an admin and sees all data
            // - guestuser@mycompany.com is not an admin and only sees data for their own region
            claims.IsAdmin = claims.Email.ToLower().Contains("admin");
            if (!claims.IsAdmin)
            {
                claims.RegionsCovered = new[] { "USA" };
            }
        }
    }
}