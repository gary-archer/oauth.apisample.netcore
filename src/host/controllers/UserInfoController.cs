namespace FinalApi.Host.Controllers
{
    using System.Linq;
    using FinalApi.Logic.Entities;
    using FinalApi.Plumbing.Claims;
    using Microsoft.AspNetCore.Mvc;

    /*
     * This user info is separate to the OpenID Connect user info that returns core user attributes
     */
    [Route("investments/userinfo")]
    public class UserInfoController : Controller
    {
        /*
         * Return product specific user info from the API to clients
         */
        [HttpGet("")]
        public ClientUserInfo GetUserInfo()
        {
            var claimsPrincipal = this.User as CustomClaimsPrincipal;

            return new ClientUserInfo(
                claimsPrincipal.Extra.Title,
                claimsPrincipal.Extra.Regions.ToArray());
        }
    }
}