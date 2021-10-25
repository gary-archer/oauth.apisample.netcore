namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
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
    public sealed class ClaimsCachingAuthorizer : IAuthorizer
    {
        private readonly ClaimsCache cache;
        private readonly OAuthAuthenticator authenticator;
        private readonly CustomClaimsProvider customClaimsProvider;

        public ClaimsCachingAuthorizer(
            ClaimsCache cache,
            OAuthAuthenticator authenticator,
            CustomClaimsProvider customClaimsProvider)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<ClaimsPrincipal> ExecuteAsync(HttpRequest request)
        {
            // Read the token first
            var accessToken = BearerToken.Read(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // On every API request we validate the JWT, in a zero trust manner
            var basePrincipal = await this.authenticator.ValidateTokenAsync(accessToken);

            // If cached results already exist for this token then return them immediately
            var accessTokenHash = this.Sha256(accessToken);
            var cachedExtraClaims = await this.cache.GetExtraUserClaimsAsync(accessTokenHash);
            if (cachedExtraClaims.Count() > 0)
            {
                return basePrincipal.ExtendClaims(cachedExtraClaims);
            }

            // In Cognito we cannot issue custom claims so the API looks them up when the access token is first received
            var userInfo = await this.authenticator.GetUserInfoAsync(accessToken);
            var customClaims = await this.customClaimsProvider.GetAsync(accessToken, basePrincipal, userInfo);
            
            // Cache the claims against the token hash until the token's expiry time
            var extraClaims = new List<Claim>();
            extraClaims.AddRange(userInfo);
            extraClaims.AddRange(customClaims);
            await this.cache.SetExtraUserClaimsAsync(accessTokenHash, extraClaims, basePrincipal.GetExpiry());

            // Extend the claims principal with all claims
            return basePrincipal.ExtendClaims(extraClaims);
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
