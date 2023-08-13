namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * Return user info from the business data to the client
     * Clients call the authorization server's user info endpoint to get OAuth user attributes
     */
    [Route("investments/userinfo")]
    public class UserInfoController : Controller
    {
        /*
         * Return attributes that are not stored in the authorization server that the UI needs
         */
        [HttpGet("")]
        public ClientUserInfo GetUserInfo()
        {
            var claimsPrincipal = this.User as CustomClaimsPrincipal;
            var extraClaims = claimsPrincipal.ExtraClaims as SampleExtraClaims;

            return new ClientUserInfo(
                extraClaims.Role,
                extraClaims.Regions.ToArray());
        }
    }
}