namespace FinalApi.Plumbing.Claims
{
    using System;
    using System.Threading.Tasks;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public interface IExtraClaimsProvider
    {
        /*
         * Get additional claims from the API's own data
         */
        Task<object> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider);

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        object DeserializeFromCache(string json);
    }
}
