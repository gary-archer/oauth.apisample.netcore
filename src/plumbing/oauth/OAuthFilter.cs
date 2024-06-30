namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;

    /*
     * A class to create the claims principal at the start of every secured request
     */
    public sealed class OAuthFilter
    {
        private readonly ClaimsCache cache;
        private readonly AccessTokenValidator accessTokenValidator;
        private readonly ExtraClaimsProvider extraClaimsProvider;

        public OAuthFilter(
            ClaimsCache cache,
            AccessTokenValidator accessTokenValidator,
            ExtraClaimsProvider extraClaimsProvider)
        {
            this.cache = cache;
            this.accessTokenValidator = accessTokenValidator;
            this.extraClaimsProvider = extraClaimsProvider;
        }

        /*
         * Validate the OAuth access token and then look up other claims
         */
        public async Task<CustomClaimsPrincipal> ExecuteAsync(HttpRequest request)
        {
            // First read the access token
            var accessToken = BearerToken.Read(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // On every API request we validate the JWT, in a zero trust manner
            var jwtClaims = await this.accessTokenValidator.ValidateTokenAsync(accessToken);

            // If cached results already exist for this token then return them immediately
            var accessTokenHash = this.Sha256(accessToken);
            var extraClaims = await this.cache.GetExtraUserClaimsAsync(accessTokenHash);
            if (extraClaims != null)
            {
                return this.extraClaimsProvider.CreateClaimsPrincipal(jwtClaims, extraClaims);
            }

            // Look up extra claims not in the JWT access token when the token is first received
            extraClaims = await this.extraClaimsProvider.LookupExtraClaimsAsync(jwtClaims, request.HttpContext.RequestServices);

            // Cache the extra claims for subsequent requests with the same access token
            await this.cache.SetExtraUserClaimsAsync(accessTokenHash, extraClaims, jwtClaims.Exp);

            // Return the final claims used by the API's authorization logic
            return this.extraClaimsProvider.CreateClaimsPrincipal(jwtClaims, extraClaims);
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
