namespace FinalApi.Host.Controllers
{
    using System.Linq;
    using FinalApi.Logic.Claims;
    using FinalApi.Logic.Entities;
    using FinalApi.Plumbing.Claims;
    using Microsoft.AspNetCore.Mvc;

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
            var extraClaims = claimsPrincipal.ExtraClaims as ExtraClaims;

            return new ClientUserInfo(
                extraClaims.Title,
                extraClaims.Regions.ToArray());
        }
    }
}