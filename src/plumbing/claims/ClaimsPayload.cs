namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * A wrapper to return both our final claims object and the token's ClaimsPrincipal
     */
    public class ClaimsPayload
    {
        public ClaimsPayload(ClaimsPrincipal principal)
        {
            this.Principal = principal;
        }

        public ClaimsPrincipal Principal { get; private set; }

        public ApiClaims ApiClaims { get; set; }
    }
}
