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
            // It is common to need to get a business user ID for the authenticated user
            // In our example a manager user may be able to view information about investors
            var managerId = this.GetManagerId(jwtClaims);

            // A real API would use a database, but this API uses a mock implementation
            var businessClaims = new List<Claim>();
            if (managerId == "20116")
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                businessClaims.Add(new Claim(CustomClaimNames.ManagerId, "20116"));
                businessClaims.Add(new Claim(CustomClaimNames.Role, "admin"));
                businessClaims.Add(new Claim(CustomClaimNames.Regions, "Europe"));
                businessClaims.Add(new Claim(CustomClaimNames.Regions, "USA"));
                businessClaims.Add(new Claim(CustomClaimNames.Regions, "Asia"));
            }
            else
            {
                // These claims are used for the guestuser@mycompany.com user account
                businessClaims.Add(new Claim(CustomClaimNames.ManagerId, "10345"));
                businessClaims.Add(new Claim(CustomClaimNames.Role, "user"));
                businessClaims.Add(new Claim(CustomClaimNames.Regions, "USA"));
            }

            return businessClaims;
        }
        #pragma warning restore 1998

        /*
         * Get a business user ID that corresponds to the user in the token
         */
        private string GetManagerId(IEnumerable<Claim> jwtClaims)
        {
            var managerId = jwtClaims.FirstOrDefault(c => c.Type == CustomClaimNames.ManagerId);
            if (managerId != null)
            {
                // The preferred option is for the API to receive the business user identity in the JWT access token
                return managerId.Value;
            }
            else
            {
                // Otherwise the API must determine the value from the subject claim
                var subject = jwtClaims.FirstOrDefault(c => c.Type == OAuthClaimNames.Subject);
                return this.LookupManagerIdFromSubjectClaim(subject.Value);
            }
        }

        /*
         * The API could store a mapping from the subject claim to the business user identity
         */
        private string LookupManagerIdFromSubjectClaim(string subject)
        {
            // A real API would use a database, but this API uses a mock implementation
            // This subject value is for the guestadmin@mycompany.com user account
            var isAdmin = subject == "77a97e5b-b748-45e5-bb6f-658e85b2df91";
            if (isAdmin)
            {
                return "20116";
            }
            else
            {
                return "10345";
            }
        }
    }
}
