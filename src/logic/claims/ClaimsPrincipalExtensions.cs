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
        public static string GetManagerId(this ClaimsPrincipal principal)
        {
            return principal.ReadStringClaim(CustomClaimNames.ManagerId).Value;
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.ReadStringClaim(CustomClaimNames.Role).Value;
        }

        public static IEnumerable<string> GetRegions(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type == CustomClaimNames.Regions).Select(s => s.Value);
        }
    }
}
