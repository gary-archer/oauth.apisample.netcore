namespace SampleApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /*
     * The claims provider class is responsible for providing the final claims object
     */
    public class CustomClaimsProvider
    {
        /*
         * This can be overridden by derived classes and is used at the time of token issuance
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> IssueAsync(string subject)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998

        /*
         * Alternatively, this can be overridden by derived classes to get custom claims when a token is first received
         */
        #pragma warning disable 1998
        public virtual async Task<IEnumerable<Claim>> GetAsync(string accessToken, ClaimsPrincipal principal, IEnumerable<Claim> userInfo)
        {
            return new List<Claim>();
        }
        #pragma warning restore 1998

        /*
         * This default implementation can be overridden to manage deserialization when claims are read from the cache
         */
        public virtual CustomClaims Deserialize(JObject claimsNode)
        {
            return CustomClaims.ImportData(claimsNode);
        }
    }
}
