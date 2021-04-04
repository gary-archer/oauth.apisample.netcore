namespace SampleApi.Host.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    /*
     * A controller called during token issuing to ask the API for custom claim values
     * This requires a capability for the Authorization Server to reach out to the API
     */
    [Route("api/customclaims")]
    public class ClaimsController : Controller
    {
        /*
         * This is called during token issuance by the Authorization Server when using the StandardAuthorizer
         * The custom claims are then included in the access token
         */
        [HttpGet("{subject}")]
        [AllowAnonymous]
        public string GetCustomClaims(string subject)
        {
            dynamic data = new JObject();
            data.user_id = "10345";
            data.user_role = "user";
            data.user_regions = new JArray();
            data.user_regions.Add("USA");
            return data.ToString();
        }
    }
}
