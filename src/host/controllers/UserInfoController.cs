namespace SampleApi.Host.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * A simple API controller to return user info
     */
    [Route("api/userclaims")]
    public class UserInfoController : Controller
    {
        private readonly UserInfoClaims claims;

        public UserInfoController(UserInfoClaims claims)
        {
            this.claims = claims;
        }

        /*
         * Return user info to the UI
         */
        [HttpGet("current")]
        public ClientUserInfo GetUserClaims()
        {
            return new ClientUserInfo(this.claims.GivenName, this.claims.FamilyName);
        }
    }
}