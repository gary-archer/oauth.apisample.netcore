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
         * This is called to get extra claims when the token is first received
         */
        #pragma warning disable 1998
        public override async Task<IEnumerable<Claim>> LookupBusinessClaimsAsync(
            string accessToken,
            IEnumerable<Claim> jwtClaims)
        {
            var subject = jwtClaims.FirstOrDefault(c => c.Type == OAuthClaimNames.Subject)?.Value;
            return this.Get(subject, string.Empty);
        }
        #pragma warning restore 1998

        /*
         * Receive user attributes from identity data, and return user attributes from business data
         */
        #pragma warning disable 1998
        private IEnumerable<Claim> Get(string subject, string email)
        {
            var claims = new List<Claim>();

            // A real system would do a database lookup here
            var isAdmin = email.Contains("admin");
            if (isAdmin)
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                claims.Add(new Claim(CustomClaimNames.ManagerId, "20116"));
                claims.Add(new Claim(CustomClaimNames.Role, "admin"));
                claims.Add(new Claim(CustomClaimNames.Regions, "Europe"));
                claims.Add(new Claim(CustomClaimNames.Regions, "USA"));
                claims.Add(new Claim(CustomClaimNames.Regions, "Asia"));
            }
            else
            {
                // For other users we hard code this user id, assign a role of 'user' and grant access to only one region
                // The CompanyService class will use these claims to return only transactions for the US region
                claims.Add(new Claim(CustomClaimNames.ManagerId, "10345"));
                claims.Add(new Claim(CustomClaimNames.Role, "user"));
                claims.Add(new Claim(CustomClaimNames.Regions, "USA"));
            }

            return claims;
        }
        #pragma warning restore 1998
    }
}
