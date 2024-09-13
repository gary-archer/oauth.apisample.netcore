namespace FinalApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * The claims of interest from the JWT access token
     */
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims, ClaimsIdentity identity)
            : base(identity)
        {
            this.JwtClaims = jwtClaims;
            this.ExtraClaims = extraClaims;
        }

        public JwtClaims JwtClaims { get; private set; }

        public ExtraClaims ExtraClaims { get; private set; }
    }
}
