namespace FinalApi.Logic.Claims
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using FinalApi.Logic.Repositories;
    using FinalApi.Plumbing.Claims;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public class ExtraClaimsProviderImpl : IExtraClaimsProvider
    {
        /*
         * Get additional claims from the API's own data
         */
        #pragma warning disable 1998
        public async Task<object> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider)
        {
            // Get the user repository for this request
            var userRepository = (UserRepository)serviceProvider.GetService(typeof(UserRepository));

            // The manager ID is a business user identity from which other claims can be looked up
            var managerId = jwtClaims.GetStringClaim(CustomClaimNames.ManagerId);
            return userRepository.GetUserInfoForManagerId(managerId);
        }
        #pragma warning restore 1998

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public object DeserializeFromCache(string json)
        {
            return JsonSerializer.Deserialize<ExtraClaims>(json);
        }
    }
}
