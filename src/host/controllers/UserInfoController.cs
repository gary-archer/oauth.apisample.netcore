namespace SampleApi.Host.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Entities;

    /*
     * A simple API controller to return user info
     */
    [Route("api/userclaims")]
    public class UserInfoController : Controller
    {
        private readonly SampleApiClaims claims;

        public UserInfoController(SampleApiClaims claims)
        {
            this.claims = claims;
        }

        /*
         * Return user info to the UI
         */
        [HttpGet("current")]
        public UserInfoClaims GetUserClaims()
        {
            return new UserInfoClaims(this.claims.GivenName, this.claims.FamilyName, this.claims.Email);
        }
    }
}