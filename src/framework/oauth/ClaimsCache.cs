namespace Framework.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Framework.Configuration;

    /*
     * Encapsulate getting and setting claims from the cache
     */
    public sealed class ClaimsCache<TClaims> where TClaims: CoreApiClaims
    {
        private readonly IDistributedCache cache;
        private readonly OAuthConfiguration configuration;
        private readonly ILogger logger;

        public ClaimsCache(
            IDistributedCache cache,
            OAuthConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            this.cache = cache;
            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<ClaimsCache<TClaims>>();
        }

        /*
         * Add our custom claims to the cache
         */
        public async Task AddClaimsForTokenAsync(string accessToken, int tokenExpirySeconds, TClaims claims)
        {
            // Check for a race condition where the token passes validation but it expired when it gets here
            var now = DateTimeOffset.UtcNow;
            var epochSeconds = now.ToUnixTimeSeconds();
            var secondsToCache = tokenExpirySeconds - epochSeconds;
            if (secondsToCache > 0)
            {
                // Get the hash and output debug info
                var hash = Sha256(accessToken);
                this.logger.LogDebug($"Token to be cached will expire in {secondsToCache} seconds (hash: {hash})");

                // Do not exceed the maximum time we configured
                var maxDuration = this.configuration.DefaultTokenCacheMinutes;
                if (secondsToCache > this.configuration.DefaultTokenCacheMinutes * 60)
                {
                    secondsToCache = this.configuration.DefaultTokenCacheMinutes * 60;
                }
                
                // Serialize claims to bytes
                var json = JsonConvert.SerializeObject(claims);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Cache the token until the above time                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = now.AddSeconds(secondsToCache)
                };

                this.logger.LogDebug($"Adding token to claims cache for {secondsToCache} seconds (hash: {hash})");
                await this.cache.SetAsync(hash, bytes, options).ConfigureAwait(false);
            }
        }

        /*
         * Read our custom claims from the cache or return null if not found
         */
        public async Task<TClaims> GetClaimsForTokenAsync(string accessToken)
        {
            // Get the hash as a cache key and see if it exists in the cache
            var hash = Sha256(accessToken);
            var bytes = await this.cache.GetAsync(hash).ConfigureAwait(false);
            if (bytes == null)
            {
                this.logger.LogDebug($"New token will be added to claims cache (hash: {hash})");
                return null;
            }

            // Deserialize and return the cached data
            this.logger.LogDebug($"Found existing token in claims cache (hash: {hash})");
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<TClaims>(json);
        }

        /*
         * Get the hash of an input string
         */
        private static string Sha256(string input)
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
