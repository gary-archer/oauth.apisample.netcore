namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;

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
            return new ClientUserInfo(
                this.User.GetRole(),
                this.User.GetRegions().ToArray());
        }
    }
}