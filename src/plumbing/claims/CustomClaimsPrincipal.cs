namespace FinalApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * The total set of claims for this API
     */
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(JwtClaims jwtClaims, object extraClaims, ClaimsIdentity identity)
            : base(identity)
        {
            this.JwtClaims = jwtClaims;
            this.ExtraClaims = extraClaims;
        }

        public JwtClaims JwtClaims { get; private set; }

        public object ExtraClaims { get; private set; }
    }
}
