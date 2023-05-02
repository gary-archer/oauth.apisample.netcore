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
            return this.Get(subject, email);
        }
        #pragma warning restore 1998

        /*
         * When using the StandardAuthorizer, this is called to read claims from the access token
         */
        #pragma warning disable 1998
        public override IEnumerable<Claim> GetFromPayload(ClaimsModel claimsModel)
        {
            return this.Get(claimsModel.Sub, claimsModel.Email);
        }
        #pragma warning restore 1998

        /*
         * When using the ClaimsCachingAuthorizer, this is called to get extra claims when the token is first received
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> GetFromLookupAsync(
            string accessToken,
            IEnumerable<Claim> baseClaims,
            IEnumerable<Claim> userInfoClaims)
        {
            var subject = baseClaims.FirstOrDefault(c => c.Type == OAuthClaimNames.Subject)?.Value;
            var email = userInfoClaims.FirstOrDefault(c => c.Type == OAuthClaimNames.Email)?.Value;
            return this.Get(subject, email);
        }
        #pragma warning restore 1998

        /*
         * Receive user attributes from identity data, and return user attributes from business data
         */
        private IEnumerable<Claim> Get(string subject, string email)
        {
            var claims = new List<Claim>();

            // A real system would do a database lookup here
            var isAdmin = email.Contains("admin");
            if (isAdmin)
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                claims.Add(new Claim(CustomClaimNames.UserId, "20116"));
                claims.Add(new Claim(CustomClaimNames.UserRole, "admin"));
                claims.Add(new Claim(CustomClaimNames.UserRegions, "Europe"));
                claims.Add(new Claim(CustomClaimNames.UserRegions, "USA"));
                claims.Add(new Claim(CustomClaimNames.UserRegions, "Asia"));
            }
            else
            {
                // For other users we hard code this user id, assign a role of 'user' and grant access to only one region
                // The CompanyService class will use these claims to return only transactions for the US region
                claims.Add(new Claim(CustomClaimNames.UserId, "10345"));
                claims.Add(new Claim(CustomClaimNames.UserRole, "user"));
                claims.Add(new Claim(CustomClaimNames.UserRegions, "USA"));
            }

            return claims;
        }
    }
}
