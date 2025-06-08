namespace FinalApi.Logic.Claims
{
    using System;
    using System.Threading.Tasks;
    using FinalApi.Logic.Repositories;
    using FinalApi.Plumbing.Claims;

    /*
     * Add extra authorization values that you cannot, or do not want to, manage in the authorization server
     */
    public class ExtraClaimsProvider : IExtraClaimsProvider
    {
        /*
         * Get extra values from the API's own data
         */
        #pragma warning disable 1998
        public async Task<ExtraClaims> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider)
        {
            // Get an object to look up user information
            var userRepository = (UserRepository)serviceProvider.GetService(typeof(UserRepository));

            // Look up values using the manager ID, a business user identity
            var managerId = jwtClaims.GetStringClaim(ClaimNames.ManagerId);
            return userRepository.GetUserInfoForManagerId(managerId);
        }
        #pragma warning restore 1998
    }
}
