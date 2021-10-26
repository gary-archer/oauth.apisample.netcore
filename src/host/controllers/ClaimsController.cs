namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SampleApi.Host.Claims;

    /*
     * A controller called during token issuing to ask the API for custom claim values
     * This requires a capability for the Authorization Server to reach out to the API
     */
    [Route("api/customclaims")]
    public class ClaimsController : Controller
    {
        private readonly SampleCustomClaimsProvider customClaimsProvider;

        public ClaimsController(SampleCustomClaimsProvider customClaimsProvider)
        {
            this.customClaimsProvider = customClaimsProvider as SampleCustomClaimsProvider;
        }

        /*
         * This is called during token issuance by the Authorization Server when using the StandardAuthorizer
         * The custom claims are then included in the access token
         */
        [HttpGet("{subject}")]
        public async Task<ContentResult> GetCustomClaims(string subject)
        {
            var customClaims = await this.customClaimsProvider.IssueAsync(subject);

            var userId = customClaims.First(c => c.Type == CustomClaimNames.UserId).Value;
            var userRole = customClaims.First(c => c.Type == CustomClaimNames.UserRole).Value;
            var userRegions = customClaims.First(c => c.Type == CustomClaimNames.UserRegions).Value;

            var data = new
            {
                user_id = userId,
                user_role = userRole,
                user_regions = userRegions.Split(' '),
            };

            return this.Content(JsonConvert.SerializeObject(data), "application/json");
        }
    }
}
