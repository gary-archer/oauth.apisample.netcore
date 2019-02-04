namespace BasicApi.Logic
{
    using Microsoft.AspNetCore.Mvc;
    using BasicApi.Entities;
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
            return this.User.GetApiClaims().UserInfo;
        }
    }
}