namespace Framework.Api.Base.Security
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Claims;
    using Microsoft.AspNetCore.Http;

    /*
     * A simpler header authorizer used by private APIs
     */
    internal sealed class HeaderAuthorizer : IAuthorizer
    {
        private readonly HeaderAuthenticator authenticator;

        public HeaderAuthorizer(HeaderAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        /*
         * The entry point for implementing authorization
         */
        public async Task<CoreApiClaims> Execute(HttpRequest request)
        {
            return await this.authenticator.AuthorizeRequestAndGetClaims(request);
        }
    }
}
