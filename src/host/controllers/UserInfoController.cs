namespace SampleApi.Host.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Entities;
    using SampleApi.Plumbing.Claims;

    /*
     * A simple API controller to return user info
     */
    [Route("api/userinfo")]
    public class UserInfoController : Controller
    {
        private readonly BaseClaims baseClaims;
        private readonly UserInfoClaims userInfoClaims;

        public UserInfoController(BaseClaims baseClaims, UserInfoClaims userInfoClaims)
        {
            this.baseClaims = baseClaims;
            this.userInfoClaims = userInfoClaims;
        }

        /*
         * Return user info to the UI
         */
        [HttpGet("")]
        public ClientUserInfo GetUserClaims()
        {
            // First check we have access to this level of data
            this.baseClaims.VerifyScope("profile");

            // Next return the user info
            return new ClientUserInfo(this.userInfoClaims.GivenName, this.userInfoClaims.FamilyName);
        }
    }
}