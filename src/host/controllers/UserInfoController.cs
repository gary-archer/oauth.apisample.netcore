namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * Return user info from the business data to the client
     * These values are separate to the core identity data returned from the OAuth user info endpoint
     */
    [Route("investments/userinfo")]
    public class UserInfoController : Controller
    {
        /*
         * Return user attributes that are not stored in the authorization server that the UI needs
         */
        [HttpGet("")]
        public ClientUserInfo GetUserInfo()
        {
            var claimsPrincipal = this.User as CustomClaimsPrincipal;
            var extraClaims = claimsPrincipal.ExtraClaims as SampleExtraClaims;

            return new ClientUserInfo(
                extraClaims.Title,
                extraClaims.Regions.ToArray());
        }
    }
}