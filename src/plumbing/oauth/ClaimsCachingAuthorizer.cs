namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Security;

    /*
     * An authorizer that manages claims in an extensible manner, with the ability to use claims from the API's own data
     */
    internal sealed class ClaimsCachingAuthorizer : IAuthorizer
    {
        private readonly ClaimsCache cache;
        private readonly OAuthAuthenticator authenticator;
        private readonly ClaimsProvider customClaimsProvider;

        public ClaimsCachingAuthorizer(
            ClaimsCache cache,
            OAuthAuthenticator authenticator,
            ClaimsProvider customClaimsProvider)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<ApiClaims> ExecuteAsync(HttpRequest request)
        {
            // First handle missing tokens
            var accessToken = BearerToken.Read(request);
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

            // Validate the token and read token claims
            var token = await this.authenticator.ValidateTokenAsync(accessToken);

            // Do the work for user info lookup
            var userInfo = await this.authenticator.GetUserInfoAsync(accessToken);

            // Ask the provider to supply the final claims object
            var claims = await this.customClaimsProvider.SupplyClaimsAsync(token, userInfo);

            // Cache the claims against the token hash until the token's expiry time
            await this.cache.AddClaimsForTokenAsync(accessTokenHash, claims);
            return claims;
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
