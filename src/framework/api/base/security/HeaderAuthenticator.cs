namespace Framework.Api.Base.Security
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Claims;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Utilities;
    using Microsoft.AspNetCore.Http;

    /*
     * An alternative authenticator for private APIs that reads headers supplied by a public API
     */
    public class HeaderAuthenticator
    {
        /*
         * The entry point for implementing authorization
         * We ignore the async not required warning since it is needed by other implementations
         */
        #pragma warning disable CS1998
        public async Task<CoreApiClaims> AuthorizeRequestAndGetClaims(HttpRequest request)
        {
            var claims = new CoreApiClaims();

            // Get token claims
            var userId = this.GetHeaderClaim(request, "x-mycompany-user-id");
            var clientId = this.GetHeaderClaim(request, "x-mycompany-client-id");
            var scope = this.GetHeaderClaim(request, "x-mycompany-scope");

            // Get user info claims
            var givenName = this.GetHeaderClaim(request, "x-mycompany-given-name");
            var familyName = this.GetHeaderClaim(request, "x-mycompany-family-name");
            var email = this.GetHeaderClaim(request, "x-mycompany-email");

            // Update the claims object
            claims.SetTokenInfo(userId, clientId, scope.Split(' '));
            claims.SetCentralUserInfo(givenName, familyName, email);
            return claims;
        }

        /*
         * A helper to get an expected claim or throw an error otherwise
         */
        private string GetHeaderClaim(HttpRequest request, string name)
        {
            var header = request.GetHeader(name);
            if (!string.IsNullOrWhiteSpace(header))
            {
                return request.Headers[name];
            }

            var handler = new ErrorUtils();
            throw handler.FromMissingClaim(name);
        }
    }
}
