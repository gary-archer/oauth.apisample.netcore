namespace SampleApi.Logic.Claims
{
    using SampleApi.Plumbing.Claims;

    /*
     * Manages claims that should be issued to the access token, to ensure that it is locked down
     * When the authorization server does not support this, look up such values from extra claims
     */
    public class SampleClaimsPrincipal : CustomClaimsPrincipal
    {
        public SampleClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
            : base(jwtClaims, extraClaims)
        {
        }

        public string GetManagerId()
        {
            var managerId = this.JwtClaims.GetOptionalStringClaim(CustomClaimNames.ManagerId);
            if (!string.IsNullOrWhiteSpace(managerId))
            {
                return managerId;
            }
            else
            {
                return ((SampleExtraClaims)this.ExtraClaims).ManagerId;
            }
        }

        public string GetRole()
        {
            var role = this.JwtClaims.GetOptionalStringClaim(CustomClaimNames.Role);
            if (!string.IsNullOrWhiteSpace(role))
            {
                return role;
            }
            else
            {
                return ((SampleExtraClaims)this.ExtraClaims).Role;
            }
        }
    }
}
