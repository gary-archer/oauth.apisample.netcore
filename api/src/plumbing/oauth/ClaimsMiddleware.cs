namespace BasicApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using IdentityModel.AspNetCore.OAuth2Introspection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using BasicApi.Entities;
    using BasicApi.Logic;
    using BasicApi.Plumbing.Utilities;

    /*
     * The entry point class for claims processing
     */
    public class ClaimsMiddleware
    {
        /*
         * Store the inner middleware
         */
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IDistributedCache cache;
        private readonly AuthorizationMicroservice authorizationMicroservice;

        /*
         * Construct and call the base class
         */
        public ClaimsMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IDistributedCache cache,
            AuthorizationMicroservice authorizationMicroservice)
        {
            this.next = next;
            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<ClaimsMiddleware>();
            this.cache = cache;
            this.authorizationMicroservice = authorizationMicroservice;
        }

        /*
         * Handle claims and then move to the next middleware
         */
        public async Task Invoke(HttpContext context)
        {
            // Get the access token
            string accessToken = TokenRetrieval.FromAuthorizationHeader()(context.Request);

            // See if claims for this token exist in the cache
            ApiClaims claims = await this.cache.GetClaimsMappedToTokenAsync(accessToken, logger);
            if (claims == null)
            {
                // Get details from the claims principal produced by Identity Model introspection
                claims = context.User.GetApiClaims();

                // Look up central user data claims
                var userInfoClient = new UserInfoHttpClient(this.configuration);
                await userInfoClient.LookupCentralUserDataClaimsAsync(claims, accessToken);

                // Look up product specific user data claims
                await this.authorizationMicroservice.getProductClaims(claims, accessToken);
                
                // Cache results for subsequent requests with this token
                int expiry = context.User.GetAccessTokenExpirationClaim();
                await this.cache.AddClaimsMappedToTokenAsync(accessToken, expiry, claims, logger);

                // Indicate success
                logger.LogInformation("ClaimsMiddleware: completed claims lookup");
            }

            // Store our claims in the principal in order to pass them to API controllers
            context.SetAdditionalApiClaims(claims);

            // Move to the next middleware
            await this.next(context);
        }
    }
}
