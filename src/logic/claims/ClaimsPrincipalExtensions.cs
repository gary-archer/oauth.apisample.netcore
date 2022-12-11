namespace SampleApi.Logic.Claims
{
    using System.Security.Claims;
    using SampleApi.Plumbing.Claims;

    /*
     * Extensions to read the claims of interest to this API
     */
    public static class ClaimsPrincipalExtensions
    {
        /*
         * Convenience accessors
         */
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, CustomClaimNames.UserId).Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            return ClaimsReader.ReadClaim(principal, CustomClaimNames.UserRole).Value;
        }

        public static string[] GetUserRegions(this ClaimsPrincipal principal)
        {
            var regionsValue = ClaimsReader.ReadClaim(principal, CustomClaimNames.UserRegions).Value;
            return regionsValue.Split(' ');
        }
    }
}
