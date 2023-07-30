namespace SampleApi.Host.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.OAuth;

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
            // First check we have access to this level of data
            ScopeVerifier.Enforce(this.User.GetScopes(), "profile");

            // Next return the user info, including the custom regions field
            return new ClientUserInfo(
                this.User.GetUserRole(),
                this.User.GetUserRegions().ToArray());
        }
    }
}