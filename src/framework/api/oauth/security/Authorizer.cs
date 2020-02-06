namespace Framework.Api.OAuth.Security
{
    using System.Linq;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.OAuth.Claims;
    using Microsoft.AspNetCore.Http;

    /*
     * The entry point for the processing to validate tokens and return claims
     * Our approach provides extensible claims to our API and enables high performance
     * It also takes close control of error responses to our SPA
     */
    public sealed class Authorizer<TClaims>
        where TClaims : CoreApiClaims, new()
    {
        private readonly ClaimsCache<TClaims> cache;
        private readonly Authenticator authenticator;
        private readonly CustomClaimsProvider<TClaims> customClaimsProvider;

        public Authorizer(
            ClaimsCache<TClaims> cache,
            Authenticator authenticator,
            CustomClaimsProvider<TClaims> customClaimsProvider)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<TClaims> Execute(HttpRequest request)
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
