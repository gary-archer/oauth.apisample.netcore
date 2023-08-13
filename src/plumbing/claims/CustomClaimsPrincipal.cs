namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * The claims of interest from the JWT access token
     */
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
            : base(GetClaimsIdentity(jwtClaims, extraClaims))
        {
            this.JwtClaims = jwtClaims;
            this.ExtraClaims = extraClaims;
        }

        public JwtClaims JwtClaims { get; private set; }

        public ExtraClaims ExtraClaims { get; private set; }

        /*
         * Wire up the claims principal in Microsoft terms, so that authorization attributes work as expected
         */
        private static ClaimsIdentity GetClaimsIdentity(JwtClaims jwtClaims, ExtraClaims extraClaims)
        {
            var identity = new ClaimsIdentity("Bearer", jwtClaims.GetNameClaimType(), extraClaims.GetRoleClaimType());
            jwtClaims.AddClaims(identity);
            extraClaims.AddClaims(identity);
            return identity;
        }
    }
}
