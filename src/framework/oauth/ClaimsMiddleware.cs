namespace Framework.OAuth
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /*
     * The entry point for the processing to validate tokens and return claims
     * Our approach provides extensible claims to our API and enables high performance
     * It also takes close control of error responses to our SPA 
     */
    public sealed class ClaimsMiddleware<TClaims> where TClaims: CoreApiClaims, new()
    {

        private readonly ClaimsCache<TClaims> cache;
        private readonly Authenticator authenticator;
        private readonly CustomClaimsProvider<TClaims> customClaimsProvider;
        private readonly ILogger logger;

        public ClaimsMiddleware(
            ClaimsCache<TClaims> cache,
            Authenticator authenticator,
            CustomClaimsProvider<TClaims> customClaimsProvider,
            ILoggerFactory loggerFactory)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
            this.logger = loggerFactory.CreateLogger<ClaimsMiddleware<TClaims>>();
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<TClaims> authorizeRequestAndGetClaims(string accessToken) {
            
            // First handle missing tokens
            if (string.IsNullOrWhiteSpace(accessToken)) 
            {
                return null;
            }

            // Bypass validation and use cached results if they exist
            var cachedClaims = await this.cache.GetClaimsForTokenAsync(accessToken);
            if (cachedClaims != null) {
                this.logger.LogInformation("Claims Middleware: Existing claims returned from cache");
                return cachedClaims;
            }

            // Otherwise create new claims which we will populate
            var claims = new TClaims();

            // Start by introspecting the token
            var result = await this.authenticator.ValidateTokenAndSetClaims(accessToken, claims);
            var tokenSuccess = result.Item1;
            var tokenExpiry = result.Item2;
            if (!tokenSuccess) {
                this.logger.LogInformation("Claims Middleware: Invalid or expired access token");
                return null;
            }

            // Next add central user info to the user's claims
            var userInfoSuccess = await this.authenticator.SetCentralUserInfoClaims(accessToken, claims);
            if (!userInfoSuccess) {
                this.logger.LogInformation("Claims Middleware: Expired access token used for user info lookup");
                return null;
            }

            // Look up any product specific custom claims if required
            await this.customClaimsProvider.AddCustomClaimsAsync(accessToken, claims);

            // Cache the claims against the token hash until the token's expiry time
            // The next time the API is called, all of the above results can be quickly looked up
            await cache.AddClaimsForTokenAsync(accessToken, tokenExpiry, claims);
            this.logger.LogInformation("Claims Middleware: Claims lookup completed successfully");
            return claims;
        }
    }
}
