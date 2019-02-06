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
        /*
         * Return user info to the UI
         */
        [HttpGet("current")]
        public UserInfoClaims GetUserClaimsAsync()
        {
            var claims = this.User.GetApiClaims();
            return new UserInfoClaims(claims.GivenName, claims.FamilyName, claims.Email);
        }
    }
}