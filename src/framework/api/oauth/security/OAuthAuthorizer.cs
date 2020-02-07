namespace Framework.Api.OAuth.Security
{
    using System.Linq;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Security;
    using Framework.Api.OAuth.Claims;
    using Microsoft.AspNetCore.Http;

    /*
     * The technology neutral algorithm for validating access tokens and returning claims
     */
    public sealed class OAuthAuthorizer<TClaims> : IAuthorizer
        where TClaims : CoreApiClaims, new()
    {
        private readonly ClaimsCache<TClaims> cache;
        private readonly OAuthAuthenticator authenticator;
        private readonly CustomClaimsProvider<TClaims> customClaimsProvider;

        public OAuthAuthorizer(
            ClaimsCache<TClaims> cache,
            OAuthAuthenticator authenticator,
            CustomClaimsProvider<TClaims> customClaimsProvider)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<CoreApiClaims> Execute(HttpRequest request)
        {
            // First handle missing tokens
            var accessToken = this.ReadAccessToken(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ClientError.Create401("No access token was received in the bearer header");
            }

            // Bypass validation and use cached results if they exist
            var cachedClaims = await this.cache.GetClaimsForTokenAsync(accessToken);
            if (cachedClaims != null)
            {
                return cachedClaims;
            }

            // Otherwise create new claims which we will populate
            var claims = new TClaims();

            // As the authenticator to do the OAuth work
            var tokenExpiry = await this.authenticator.AuthenticateAndSetClaims(accessToken, request, claims);

            // Look up any product specific custom claims if required
            await this.customClaimsProvider.AddCustomClaimsAsync(accessToken, claims);

            // Cache the claims against the token hash until the token's expiry time
            await this.cache.AddClaimsForTokenAsync(accessToken, tokenExpiry, claims);

            // Return the final claims
            return claims;
        }

        /*
         * Try to read the bearer token from the authorization header
         */
        private string ReadAccessToken(HttpRequest request)
        {
            string authorization = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0] == "Bearer")
                {
                    return parts[1];
                }
            }

            return null;
        }
    }
}
