namespace SampleApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /*
     * The claims provider class is responsible for providing the final claims object
     */
    public class CustomClaimsProvider
    {
        /*
         * When using the StandardAuthorizer this is called at the time of token issuance
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> IssueAsync(string subject, string email)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998

        /*
         * When using the StandardAuthorizer, this is called to read claims from the access token
         */
        #pragma warning disable 1998
        public virtual IEnumerable<Claim> GetFromPayload(ClaimsModel claimsModel)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998

        /*
         * When using the ClaimsCachingAuthorizer, this is called to get extra claims when the token is first received
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> GetFromLookupAsync(
            string accessToken,
            IEnumerable<Claim> baseClaims,
            IEnumerable<Claim> userInfoClaims)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998
    }
}
