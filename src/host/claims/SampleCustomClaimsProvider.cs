namespace SampleApi.Host.Claims
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * A custom claims provider to include extra domain specific claims in the claims cache
     */
    public class SampleCustomClaimsProvider : CustomClaimsProvider
    {
        /*
        * When using the StandardAuthorizer this is called at the time of token issuance by the ClaimsController
        * My Authorization Server setup currently sends the user's email as the subject claim
        */
        #pragma warning disable 1998
        public async Task<SampleCustomClaims> SupplyCustomClaimsFromSubjectAsync(string subject)
        {
            return this.SupplyCustomClaims(subject) as SampleCustomClaims;
        }
        #pragma warning restore 1998

        /*
         * An example of how custom claims can be included
         */
        protected override Task<CustomClaims> SupplyCustomClaimsAsync(
            ClaimsPayload tokenData,
            ClaimsPayload userInfoData)
        {
            var email = userInfoData.GetClaim("email");
            var claims = this.SupplyCustomClaims(email);
            return Task.FromResult(claims);
        }

        /*
         * When using the StandardAuthorizer we read all custom claims directly from the token
         */
        protected override CustomClaims ReadCustomClaims(ClaimsPayload payload)
        {
            /*const userId = token.getClaim('user_id');
            const role = token.getClaim('user_role');
            const userRegions = token.getClaim('user_regions');
            return new SampleCustomClaims(userId, role, userRegions);*/

            throw new NotImplementedException("Not implemented");
        }

        /*
         * Override to call the correct concrete class for this API
         */
        protected override CustomClaims DeserializeCustomClaims(JObject claimsNode)
        {
            return SampleCustomClaims.ImportData(claimsNode);
        }

        /*
         * Simulate some API logic for identifying the user from OAuth data, via either the subject or email claims
         * A real API would then do a database lookup to find the user's custom claims
         */
        private CustomClaims SupplyCustomClaims(string email)
        {
            var isAdmin = email.ToLowerInvariant().Contains("admin");
            if (isAdmin)
            {
                // For admin users we hard code this user id, assign a role of 'admin' and grant access to all regions
                // The CompanyService class will use these claims to return all transaction data
                return new SampleCustomClaims("20116", "admin", new string[] { });
            }
            else
            {
                // For other users we hard code this user id, assign a role of 'user' and grant access to only one region
                // The CompanyService class will use these claims to return only transactions for the US region
                return new SampleCustomClaims("10345", "user", new string[] { "USA" });
            }
        }
    }
}
