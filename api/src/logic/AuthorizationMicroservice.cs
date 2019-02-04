namespace BasicApi.Logic
{
    using System.Threading.Tasks;
    using BasicApi.Entities;

    /*
     * A repository class for returning domain specific authorization rules
     */
    public class AuthorizationMicroservice
    {
        /*
         * Return custom domain specific claims given the token claims and central user claims
         * If required the access token could be used to call an authorization microservice
         */
        public Task getProductClaims(ApiClaims claims, string accessToken)
        {
            claims.setProductSpecificUserRights(this.CompaniesCoveredByUser(claims.UserId));
            return Task.FromResult(0);
        }

        /*
         * For the purposes of our code sample we will grant access to the first 3 companies
         * However, the API will deny access to company 4, which the user does not have rights to
         */
        private int[] CompaniesCoveredByUser(string userId)
        {
            return new int[] {1, 2, 3};
        }
    }
}