namespace SampleApi.Host.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SampleApi.Host.Claims;
    using SampleApi.Logic.Entities;

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
            var customClaims = (SampleCustomClaims)await this.customClaimsProvider.IssueAsync(subject);

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
