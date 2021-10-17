namespace SampleApi.Host.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SampleApi.Host.Claims;
    using SampleApi.Plumbing.Claims;

    /*
     * A controller called during token issuing to ask the API for custom claim values
     * This requires a capability for the Authorization Server to reach out to the API
     */
    [Route("api/customclaims")]
    public class ClaimsController : Controller
    {
        private readonly SampleClaimsProvider claimsProvider;

        public ClaimsController(ClaimsProvider claimsProvider)
        {
            this.claimsProvider = claimsProvider as SampleClaimsProvider;
        }

        /*
         * This is called during token issuance by the Authorization Server when using the StandardAuthorizer
         * The custom claims are then included in the access token
         */
        [HttpGet("{subject}")]
        [AllowAnonymous]
        public async Task<ContentResult> GetCustomClaims(string subject)
        {
            var customClaims = await this.claimsProvider.SupplyCustomClaimsFromSubjectAsync(subject);

            var data = new
            {
                user_id = customClaims.UserId,
                user_role = customClaims.UserRole,
                user_regions = customClaims.UserRegions,
            };

            return this.Content(JsonConvert.SerializeObject(data), "application/json");
        }
    }
}
