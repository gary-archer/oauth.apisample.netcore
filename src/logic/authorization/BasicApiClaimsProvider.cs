namespace BasicApi.Logic.Authorization
{
    using System.Threading.Tasks;
    using Framework.OAuth;
    using BasicApi.Logic.Entities;

    /*
     * A custom claims provider to include extra domain specific claims in the claims cache
     */
    public class BasicApiClaimsProvider : CustomClaimsProvider<BasicApiClaims>
    {
        /*
         * The interface supports returning results based on the user id from the token
         * This might involve a database lookup or a call to another service
         */
        public override Task AddCustomClaimsAsync(string accessToken, BasicApiClaims claims)
        {
            // Any attempts to access data for company 3 will result in an unauthorized error
            claims.AccountsCovered = new int[]{1, 2, 4};
            return Task.FromResult(0);
        }
    }
}