namespace BasicApi.Logic.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BasicApi.Logic.Entities;

    /// <summary>
    /// A simple API controller to return user info
    /// </summary>
    [Route("api/userclaims")]
    public class UserInfoController : Controller
    {
        private readonly BasicApiClaims claims;

        /// <summary>
        /// Receive dependencies
        /// </summary>
        /// <param name="claims">The claims for the user in the token</param>
        public UserInfoController(BasicApiClaims claims)
        {
            this.claims = claims;
        }

        /// <summary>
        /// Return user info to the UI
        /// </summary>
        /// <returns>A user info object</returns>
        [HttpGet("current")]
        public UserInfoClaims GetUserClaims()
        {
            return new UserInfoClaims(this.claims.GivenName, this.claims.FamilyName, this.claims.Email);
        }
    }
}