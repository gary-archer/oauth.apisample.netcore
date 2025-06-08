namespace FinalApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * The claims for this API
     */
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims, ClaimsIdentity identity)
            : base(identity)
        {
            this.Jwt = jwtClaims;
            this.Extra = extraClaims;
        }

        public JwtClaims Jwt { get; private set; }

        public ExtraClaims Extra { get; private set; }
    }
}
