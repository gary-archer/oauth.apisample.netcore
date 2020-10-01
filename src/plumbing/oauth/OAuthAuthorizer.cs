namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Security;

    /*
     * The technology neutral algorithm for validating access tokens and returning claims
     */
    internal sealed class OAuthAuthorizer<TClaims> : IAuthorizer
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
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // If cached results already exist for this token then return them immediately
            var accessTokenHash = this.Sha256(accessToken);
            var cachedClaims = await this.cache.GetClaimsForTokenAsync(accessTokenHash);
            if (cachedClaims != null)
            {
                return cachedClaims;
            }

            // Otherwise create new claims which we will populate
            var claims = new TClaims();

            // Validate the token, read token claims, and do a user info lookup
            await this.authenticator.ValidateTokenAndGetClaims(accessToken, request, claims);

            // Add custom claims from the API's own data if needed
            await this.customClaimsProvider.AddCustomClaimsAsync(accessToken, claims);

            // Cache the claims against the token hash until the token's expiry time
            await this.cache.AddClaimsForTokenAsync(accessTokenHash, claims);

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

        /*
         * Get the hash of an input string
         */
        private string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
