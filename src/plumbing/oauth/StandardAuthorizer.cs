namespace SampleApi.Plumbing.OAuth
{
    using System.Collections.Generic;
    using System.Security.Claims;
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
        private readonly CustomClaimsProvider claimsProvider;

        public StandardAuthorizer(OAuthAuthenticator authenticator, CustomClaimsProvider claimsProvider)
        {
            this.authenticator = authenticator;
            this.claimsProvider = claimsProvider;
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
            var claimsModel = await this.authenticator.ValidateTokenAsync(accessToken);

            // Read claims to check that expected values exist
            var baseClaims = ClaimsReader.BaseClaims(claimsModel);
            var userInfoClaims = ClaimsReader.UserInfoClaims(claimsModel);
            var customClaims = this.claimsProvider.GetFromPayload(claimsModel);

            var allClaims = new List<Claim>();
            allClaims.AddRange(baseClaims);
            allClaims.AddRange(userInfoClaims);
            allClaims.AddRange(customClaims);

            // Create a claims principal so that Microsoft authorization works
            var identity = new ClaimsIdentity(allClaims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }
}
