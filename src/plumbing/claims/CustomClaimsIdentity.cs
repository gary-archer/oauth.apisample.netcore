namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;

    /*
     * API claims used for authorization
     */
    public class CustomClaimsIdentity : ClaimsIdentity
    {
        public CustomClaimsIdentity(ApiClaims claims)
        {
            this.ApiClaims = claims;
        }

        public ApiClaims ApiClaims { get; set; }
    }
}
