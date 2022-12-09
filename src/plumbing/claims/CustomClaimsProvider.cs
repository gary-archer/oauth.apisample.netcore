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
         * This can be overridden when an API wants to provide a domain specific claims endpoint
         * The authorization server then calls the API at the time of token issuance
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> IssueAsync(string subject, string email)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998

        /*
         * This can be overridden by an API that looks up extra claims from a cache
         * This is used when the authorization server does not support issuing domain specific claims
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> GetAsync(string accessToken, ClaimsPrincipal principal, IEnumerable<Claim> userInfo)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998
    }
}
