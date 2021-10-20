namespace SampleApi.Host.Claims
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Logic.Entities;
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
        public override async Task<CustomClaims> IssueAsync(string subject)
        {
            return this.GetCustomClaims(subject);
        }
        #pragma warning restore 1998

        /*
         * When using the ClaimsCachingAuthorizer this is called when an API first receives the access token
         */
        #pragma warning disable 1998
        public override async Task<CustomClaims> GetAsync(string accessToken, BaseClaims baseClaims, UserInfoClaims userInfo)
        {
            return this.GetCustomClaims(userInfo.Email);
        }
        #pragma warning restore 1998

        /*
         * Ensure that custom claims are correctly deserialized
         */
        public override CustomClaims Deserialize(JObject claimsNode)
        {
            return SampleCustomClaims.ImportData(claimsNode);
        }

        /*
         * Simulate some API logic for identifying the user from OAuth data, via either the subject or email claims
         * A real API would then do a database lookup to find the user's custom claims
         */
        private CustomClaims GetCustomClaims(string email)
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
