namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using SampleApi.Logic.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.OAuth;

    /*
     * A controller called during token issuing to ask the API for custom claim values
     * This requires a capability for the Authorization Server to reach out to the API
     */
    [Route("api/customclaims")]
    public class ClaimsController : Controller
    {
        private readonly OAuthConfiguration configuration;
        private readonly SampleCustomClaimsProvider customClaimsProvider;

        public ClaimsController(
            OAuthConfiguration configuration,
            SampleCustomClaimsProvider customClaimsProvider)
        {
            this.configuration = configuration;
            this.customClaimsProvider = customClaimsProvider as SampleCustomClaimsProvider;
        }

        /*
         * This is called during token issuance when the Authorization Server supports it
         * The Authorization Server will then include claims returned in the JWT access token
         */
        [AllowAnonymous]
        [HttpPost]
        public async Task<ContentResult> GetCustomClaims([FromBody] IdentityClaims identityClaims)
        {
            // The endpoint is only enabled when this claims strategy is used
            if (this.configuration.ClaimsStrategy != "jwt")
            {
                ScopeVerifier.Deny();
            }

            // Sanity checks on required input
            if (string.IsNullOrWhiteSpace(identityClaims.Subject))
            {
                throw ErrorUtils.FromMissingClaim("subject");
            }

            if (string.IsNullOrWhiteSpace(identityClaims.Email))
            {
                throw ErrorUtils.FromMissingClaim("email");
            }

            // Send identity claims and receive domain specific claims
            var subject = identityClaims.Subject;
            var email = identityClaims.Email;
            var customClaims = await this.customClaimsProvider.IssueAsync(subject, email);

            // Extract and return values
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
