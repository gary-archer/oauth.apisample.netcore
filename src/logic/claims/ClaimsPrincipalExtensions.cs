namespace SampleApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using SampleApi.Plumbing.Claims;

    /*
     * Extensions to read the claims of interest to this API
     */
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.ReadStringClaim(CustomClaimNames.UserId).Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            return principal.ReadStringClaim(CustomClaimNames.UserRole).Value;
        }

        public static IEnumerable<string> GetUserRegions(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type == CustomClaimNames.UserRegions).Select(s => s.Value);
        }
    }
}
