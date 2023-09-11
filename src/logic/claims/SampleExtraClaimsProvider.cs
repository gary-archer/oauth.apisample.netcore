namespace SampleApi.Logic.Claims
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Logic.Repositories;
    using SampleApi.Plumbing.Claims;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public class SampleExtraClaimsProvider : ExtraClaimsProvider
    {
        /*
         * Get additional claims from the API's own database
         */
        #pragma warning disable 1998
        public override async Task<ExtraClaims> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider)
        {
            // Get the user repository for this request
            var userRepository = (UserRepository)serviceProvider.GetService(typeof(UserRepository));

            // First, see which claims are included in access tokens
            var managerId = jwtClaims.GetOptionalStringClaim(CustomClaimNames.ManagerId);
            if (!string.IsNullOrWhiteSpace(managerId))
            {
                // The best model is to receive a useful user identity in access tokens, along with the user role
                // This ensures a locked down token and also simpler code
                return userRepository.GetClaimsForManagerId(managerId);
            }
            else
            {
                // With AWS Cognito, there is a lack of support for custom claims in access tokens at the time of writing
                // So the API has to map the subject to its own user identity and look up all custom claims
                return userRepository.GetClaimsForSubject(jwtClaims.Sub);
            }
        }
        #pragma warning restore 1998

        /*
         * Create a claims principal that manages lookups across both token claims and extra claims
         */
        public override CustomClaimsPrincipal CreateClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
        {
            return new SampleClaimsPrincipal(jwtClaims, extraClaims);
        }

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public override ExtraClaims DeserializeFromCache(JObject data)
        {
            return SampleExtraClaims.ImportData(data);
        }
    }
}
