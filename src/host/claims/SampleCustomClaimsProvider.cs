namespace SampleApi.Host.Claims
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * This class provides any API specific custom claims
     */
    public class SampleCustomClaimsProvider : CustomClaimsProvider
    {
        /*
         * When using the StandardAuthorizer this is called at the time of token issuance by the ClaimsController
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> IssueAsync(string subject)
        {
            return this.GetCustomClaims(subject);
        }
        #pragma warning restore 1998

        /*
         * When using the ClaimsCachingAuthorizer this is called when an API first receives the access token
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> GetAsync(string accessToken, ClaimsPrincipal basePrincipal, IEnumerable<Claim> userInfo)
        {
            var emailClaim = userInfo.First(c => c.Type == StandardClaimNames.Email);
            return this.GetCustomClaims(basePrincipal.GetSubject());
        }
        #pragma warning restore 1998

        /*
         * Determine the user in business terms from the Authorization Server's subject claim
         */
        private IEnumerable<Claim> GetCustomClaims(string subject)
        {
            var claims = new List<Claim>();
            var regions = new List<string>();

            // A real system would do a database lookup herem, but I am hard coding based on an AWS Cognito user
            var isAdmin = subject == "77a97e5b-b748-45e5-bb6f-658e85b2df91";
            if (isAdmin)
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                claims.Add(new Claim(CustomClaimNames.UserId, "20116"));
                claims.Add(new Claim(CustomClaimNames.UserRole, "admin"));
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
