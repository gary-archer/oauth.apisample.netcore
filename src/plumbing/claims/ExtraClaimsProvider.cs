namespace SampleApi.Plumbing.Claims
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public class ExtraClaimsProvider
    {
        /*
         * Get additional claims from the API's own database
         */
        #pragma warning disable 1998
        public virtual async Task<ExtraClaims> LookupBusinessClaimsAsync(
            string accessToken,
            JwtClaims jwtClaims)
        {
            return new ExtraClaims();
        }
        #pragma warning restore 1998

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public virtual ExtraClaims DeserializeFromCache(JObject data)
        {
            return ExtraClaims.ImportData(data);
        }
    }
}
