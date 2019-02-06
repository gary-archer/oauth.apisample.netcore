namespace BasicApi.Logic
{
    using System.Threading.Tasks;
    using BasicApi.Plumbing.OAuth;

    /*
     * An example of including domain specific authorization rules during claims lookup
     */
    public class AuthorizationRulesRepository
    {
        /*
         * The interface supports returning results based on the user id from the token
         * This might involve a database lookup or a call to another service
         */
        public Task AddCustomClaims(string accessToken, ApiClaims claims)
        {
            // Any attempts to access data for company 3 will result in an unauthorized error
            claims.AccountsCovered = new int[]{1, 2, 4};
            return Task.FromResult(0);
        }
    }
}