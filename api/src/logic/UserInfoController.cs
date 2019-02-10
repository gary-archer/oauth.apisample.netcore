namespace BasicApi.Logic
{
    using Microsoft.AspNetCore.Mvc;
    using BasicApi.Entities;
    using BasicApi.Plumbing.OAuth;
    using BasicApi.Plumbing.Utilities;

    /*
     * A simple API controller to return user info
     */
    [Route("api/userclaims")]
    public class UserInfoController : Controller
    {
        private readonly ApiClaims claims;

        /*
         * Receive dependencies
         */
        public UserInfoController(ApiClaims claims)
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