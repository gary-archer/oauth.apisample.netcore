namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * A simple API controller to serve user info
     */
    [Route("investments/userinfo")]
    public class UserInfoController : Controller
    {
        /*
         * Return user info to the UI
         */
        [HttpGet("")]
        public ClientUserInfo GetUserClaims()
        {
            var claimsPrincipal = this.User as CustomClaimsPrincipal;
            var extraClaims = claimsPrincipal.ExtraClaims as SampleExtraClaims;

            return new ClientUserInfo(
                extraClaims.Role,
                extraClaims.Regions.ToArray());
        }
    }
}