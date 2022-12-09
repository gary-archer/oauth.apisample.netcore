namespace SampleApi.Logic.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * A provider of domain specific claims
     */
    public class SampleCustomClaimsProvider : CustomClaimsProvider
    {
        /*
         * When using the StandardAuthorizer this is called at the time of token issuance
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> IssueAsync(string subject, string email)
        {
            return this.GetCustomClaims(subject, email);
        }
        #pragma warning restore 1998

        /*
         * When using the ClaimsCachingAuthorizer, this is called to get extra claims when the token is first received
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> GetAsync(string accessToken, ClaimsPrincipal basePrincipal, IEnumerable<Claim> userInfo)
        {
            var email = userInfo.First(c => c.Type == OAuthClaimNames.Email)?.Value;
            return this.GetCustomClaims(basePrincipal.GetSubject(), email);
        }
        #pragma warning restore 1998

        /*
         * Receive user attributes from identity data, and return user attributes from business data
         */
        private IEnumerable<Claim> GetCustomClaims(string subject, string email)
        {
            var claims = new List<Claim>();
            var regions = new List<string>();

            // A real system would do a database lookup here
            var isAdmin = email.Contains("admin");
            if (isAdmin)
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                claims.Add(new Claim(CustomClaimNames.UserId, "20116"));
                claims.Add(new Claim(CustomClaimNames.UserRole, "admin"));
                regions.Add("Europe");
                regions.Add("USA");
                regions.Add("Asia");
            }
            else
            {
                // For other users we hard code this user id, assign a role of 'user' and grant access to only one region
                // The CompanyService class will use these claims to return only transactions for the US region
                claims.Add(new Claim(CustomClaimNames.UserId, "10345"));
                claims.Add(new Claim(CustomClaimNames.UserRole, "user"));
                regions.Add("USA");
            }

            claims.Add(new Claim(CustomClaimNames.UserRegions, string.Join(' ', regions)));
            return claims;
        }
    }
}
