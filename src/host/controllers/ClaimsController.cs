namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
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
         * This is called during token issuance when the Authorization Server supports it
         * The Authorization Server will then include claims returned in the JWT access token
         */
        [AllowAnonymous]
        [HttpPost]
        public async Task<ContentResult> GetCustomClaims()
        {
            var subject = "test";
            var email = "user";
            var customClaims = await this.customClaimsProvider.IssueAsync(subject, email);

            var userId = customClaims.First(c => c.Type == CustomClaimNames.UserId).Value;
            var userRole = customClaims.First(c => c.Type == CustomClaimNames.UserRole).Value;
            var userRegions = customClaims.First(c => c.Type == CustomClaimNames.UserRegions).Value;

            var data = new
            {
                user_id = userId,
                user_role = userRole,
                user_regions = userRegions.Split(' '),
            };

            System.Console.WriteLine(JsonConvert.SerializeObject(data));
            return this.Content(JsonConvert.SerializeObject(data), "application/json");
        }
    }
}
