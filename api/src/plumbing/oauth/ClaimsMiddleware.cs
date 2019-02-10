namespace BasicApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using BasicApi.Entities;
    using BasicApi.Logic;
    using BasicApi.Plumbing.Utilities;

    /*
     * The entry point for the processing to validate tokens and return claims
     * Our approach provides extensible claims to our API and enables high performance
     * It also takes close control of error responses to our SPA
     */
    public class ClaimsMiddleware {

        /*
         * Injected dependencies
         */
        private readonly ClaimsCache cache;
        private readonly Authenticator authenticator;
        private readonly AuthorizationRulesRepository rulesRepository;
        private readonly ILogger logger;

        /*
         * Receive dependencies
         */
        public ClaimsMiddleware(
            ClaimsCache cache,
            Authenticator authenticator,
            AuthorizationRulesRepository rulesRepository,
            ILoggerFactory loggerFactory)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.rulesRepository = rulesRepository;
            this.logger = loggerFactory.CreateLogger<ClaimsMiddleware>();
        }

        /*
         * The entry point function
         */
        public async Task<bool> authorizeRequestAndSetClaims(string accessToken, ApiClaims claims) {
            
            // First report missing tokens
            if (string.IsNullOrWhiteSpace(accessToken)) 
            {
                return false;
            }

            // Bypass validation and use cached results if they exist
            var cacheSuccess = await this.cache.GetClaimsForTokenAsync(accessToken, claims);
            if (cacheSuccess) {
                this.logger.LogInformation("Claims Middleware: Existing claims returned from cache");
                return true;
            }

            // Otherwise start by introspecting the token
            var result = await this.authenticator.ValidateTokenAndSetClaims(accessToken, claims);
            var tokenSuccess = result.Item1;
            var expiry = result.Item2;
            if (!tokenSuccess) {
                this.logger.LogInformation("Claims Middleware: Invalid or expired access token");
                return false;
            }

            // Next add central user info to the user's claims
            var userInfoSuccess = await this.authenticator.SetCentralUserInfoClaims(accessToken, claims);
            if(!userInfoSuccess) {
                this.logger.LogInformation("Claims Middleware: Expired access token used for user info lookup");
                return false;
            }

            // Look up any product specific custom claims if required
            if(this.rulesRepository != null) {
                await this.rulesRepository.AddCustomClaimsAsync(accessToken, claims);
            }

            // Cache the claims against the token hash until the token's expiry time
            // The next time the API is called, all of the above results can be quickly looked up
            await cache.AddClaimsForTokenAsync(accessToken, expiry, claims);
            this.logger.LogInformation("Claims Middleware: Claims lookup completed successfully");
            return true;
        }
    }
}
