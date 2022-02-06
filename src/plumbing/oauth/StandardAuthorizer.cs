namespace SampleApi.Plumbing.OAuth
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Security;

    /*
     * An authorizer that relies on the advanced features of the Authorization Server to provide claims
     * This is the preferred option when supported, since it leads to simpler code and better security
     */
    public sealed class StandardAuthorizer : IAuthorizer
    {
        private readonly OAuthAuthenticator authenticator;

        public StandardAuthorizer(OAuthAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        /*
         * OAuth authorization involves token validation and claims lookup
         */
        public async Task<ClaimsPrincipal> ExecuteAsync(HttpRequest request)
        {
            // First handle missing tokens
            var accessToken = BearerToken.Read(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // On every API request we validate the JWT, in a zero trust manner, and read all claims from it
            return await this.authenticator.ValidateTokenAsync(accessToken);
        }
    }
}
