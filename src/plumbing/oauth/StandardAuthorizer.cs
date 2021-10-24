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
    public sealed class StandardAuthorizer : IAuthorizer
    {
        private readonly OAuthAuthenticator authenticator;
        private readonly CustomClaimsProvider customClaimsProvider;

        public StandardAuthorizer(
            OAuthAuthenticator authenticator,
            CustomClaimsProvider customClaimsProvider)
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

            // On every API request we validate the JWT, in a zero trust manner
            var payload = await this.authenticator.ValidateTokenAsync(accessToken);

            // Then read all claims from the token
            var baseClaims = ClaimsReader.BaseClaims(payload);
            var userInfo = ClaimsReader.UserInfoClaims(payload);
            var customClaims = await this.customClaimsProvider.GetAsync(accessToken, baseClaims, userInfo);
            return new ApiClaims(baseClaims, userInfo, customClaims);
        }
    }
}
