namespace Framework.Api.Base.Security
{
    using System.Threading.Tasks;
    using Framework.Api.OAuth.Claims;
    using Microsoft.AspNetCore.Http;

    /*
     * An authenticator that reads claims from headers
     */
    public class HeaderAuthorizer : IAuthorizer
    {
        /*
         * The entry point for implementing authorization
         */
        public Task<CoreApiClaims> Execute(HttpRequest request)
        {
            return null;
        }
    }
}
