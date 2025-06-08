namespace FinalApi.Plumbing.OAuth
{
    using System;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Errors;
    using Microsoft.AspNetCore.Http;

    /*
     * A class to create the claims principal at the start of every API request
     */
    public sealed class OAuthFilter
    {
        private readonly ClaimsCache cache;
        private readonly AccessTokenValidator accessTokenValidator;
        private readonly IExtraClaimsProvider extraClaimsProvider;

        public OAuthFilter(
            ClaimsCache cache,
            AccessTokenValidator accessTokenValidator,
            IExtraClaimsProvider extraClaimsProvider)
        {
            this.cache = cache;
            this.accessTokenValidator = accessTokenValidator;
            this.extraClaimsProvider = extraClaimsProvider;
        }

        /*
         * Validate the OAuth access token and then look up other values
         */
        public async Task<CustomClaimsPrincipal> ExecuteAsync(HttpRequest request)
        {
            // First read the access token
            var accessToken = BearerToken.Read(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // Validate the JWT access token on every API request, in a zero trust manner
            var jwtClaims = await this.accessTokenValidator.ValidateTokenAsync(accessToken);

            // If cached extra values already exist for this token then return them immediately
            var accessTokenHash = this.Sha256(accessToken);
            var extraClaims = await this.cache.GetItemAsync(accessTokenHash);
            if (extraClaims != null)
            {
                return this.CreateClaimsPrincipal(jwtClaims, extraClaims);
            }

            // Look up extra authorization values not in the JWT
            extraClaims = await this.extraClaimsProvider.LookupExtraClaimsAsync(jwtClaims, request.HttpContext.RequestServices);

            // Cache the extra values for subsequent requests with the same access token
            await this.cache.SetItemAsync(accessTokenHash, extraClaims, jwtClaims.Exp);

            // Return the final claims used by the API's authorization logic
            return this.CreateClaimsPrincipal(jwtClaims, extraClaims);
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

        /*
         * This API uses an object based .NET claims principal instead of string claims
         */
        private CustomClaimsPrincipal CreateClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
        {
            var identity = new ClaimsIdentity("Bearer", ClaimNames.Subject, null);
            return new CustomClaimsPrincipal(jwtClaims, extraClaims, identity);
        }
    }
}
