namespace SampleApi.Host.Authorization
{
    using System.Threading.Tasks;
    using Framework.OAuth;
    using SampleApi.Logic.Entities;

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
            // Any attempts to access data for company 3 will result in an unauthorized error
            claims.RegionsCovered = new string[]{ "Europe", "USA" };
            return Task.FromResult(0);
        }
    }
}