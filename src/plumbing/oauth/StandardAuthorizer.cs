namespace SampleApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Security;

    /*
     * An authorizer that relies on the advanced features of the Authorization Server to provide claims
     * This is the preferred option when supported, since it leads to simpler code and better security
     */
    internal sealed class StandardAuthorizer : IAuthorizer
    {
        private readonly OAuthAuthenticator authenticator;
        private readonly ClaimsProvider customClaimsProvider;

        public StandardAuthorizer(
            OAuthAuthenticator authenticator,
            ClaimsProvider customClaimsProvider)
        {
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
        }

        /*
         * OAuth authorization involves token validation and claims lookup
         */
        public async Task<ApiClaims> ExecuteAsync(HttpRequest request)
        {
            // First handle missing tokens
            var accessToken = BearerToken.Read(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // Do the token validation work
            var payload = await this.authenticator.ValidateTokenAsync(accessToken);

            // Ask the claims provider to read the payload and supply the final claims object
            return this.customClaimsProvider.ReadClaims(payload);
        }
    }
}
