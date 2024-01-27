namespace SampleApi.Logic.Claims
{
    using System;
    using System.Security.Claims;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
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

            // The manager ID is a business user identity from which other claims can be looked up
            var managerId = jwtClaims.GetStringClaim(CustomClaimNames.ManagerId);
            return userRepository.GetClaimsForManagerId(managerId);
        }
        #pragma warning restore 1998

        /*
         * Create a claims principal containing all claims
         */
        public override CustomClaimsPrincipal CreateClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
        {
            // Create the identity
            var identity = new ClaimsIdentity("Bearer", OAuthClaimNames.Subject, CustomClaimNames.Role);

            // Add generic JWT claims
            jwtClaims.AddToClaimsIdentity(identity);

            // Add custom JWT claims
            identity.AddClaim(new Claim(CustomClaimNames.ManagerId, jwtClaims.GetStringClaim(CustomClaimNames.ManagerId)));
            identity.AddClaim(new Claim(CustomClaimNames.Role, jwtClaims.GetStringClaim(CustomClaimNames.Role)));

            // Add claims not included in the JWT
            extraClaims.AddToClaimsIdentity(identity);

            // Return the final claims principal
            return new CustomClaimsPrincipal(jwtClaims, extraClaims, identity);
        }

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public override ExtraClaims DeserializeFromCache(JsonNode data)
        {
            return SampleExtraClaims.ImportData(data);
        }
    }
}
