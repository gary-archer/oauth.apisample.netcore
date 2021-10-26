namespace SampleApi.Host.Claims
{
    using System.Linq;
    using System.Security.Claims;

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
            return principal.Claims.First(c => c.Type == CustomClaimNames.UserId).Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(c => c.Type == CustomClaimNames.UserRole).Value;
        }

        public static string[] GetUserRegions(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type == CustomClaimNames.UserRegions).Select(c => c.Value).ToArray();
        }
    }
}
