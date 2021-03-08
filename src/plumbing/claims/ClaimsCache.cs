namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SampleApi.Plumbing.Configuration;

    /*
     * Encapsulate getting and setting claims from the cache
     */
    internal sealed class ClaimsCache
    {
        private readonly IDistributedCache cache;
        private readonly ClaimsConfiguration configuration;
        private readonly ILogger traceLogger;

        public ClaimsCache(
            IDistributedCache cache,
            ClaimsConfiguration configuration,
            ServiceProvider container)
        {
            this.cache = cache;
            this.configuration = configuration;

            // Get a development trace logger for this class
            this.traceLogger = container.GetService<ILoggerFactory>().CreateLogger<ClaimsCache>();
        }

        /*
         * Read our custom claims from the cache or return null if not found
         */
        public async Task<ApiClaims> GetClaimsForTokenAsync(string accessTokenHash)
        {
            // Get the hash as a cache key and see if it exists in the cache
            var bytes = await this.cache.GetAsync(accessTokenHash).ConfigureAwait(false);
            if (bytes == null)
            {
                this.traceLogger.LogDebug($"New token will be added to claims cache (hash: {accessTokenHash})");
                return null;
            }

            // Deserialization requires the claims class to have public setter properties
            this.traceLogger.LogDebug($"Found existing token in claims cache (hash: {accessTokenHash})");
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ApiClaims>(json);
        }

        /*
         * Add our custom claims to the cache
         */
        public async Task AddClaimsForTokenAsync(string accessTokenHash, ApiClaims claims)
        {
            // Check for a race condition where the token passes validation but it expired when it gets here
            var now = DateTimeOffset.UtcNow;
            var epochSeconds = now.ToUnixTimeSeconds();
            var secondsToCache = claims.Token.Expiry - epochSeconds;
            if (secondsToCache > 0)
            {
                // Get the hash and output debug info
                this.traceLogger.LogDebug($"Token to be cached will expire in {secondsToCache} seconds (hash: {accessTokenHash})");

                // Do not exceed the maximum time we configured
                if (secondsToCache > this.configuration.MaxCacheMinutes * 60)
                {
                    secondsToCache = this.configuration.MaxCacheMinutes * 60;
                }

                // Serialize claims to bytes
                var json = JsonConvert.SerializeObject(claims);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Cache the token until the above time
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = now.AddSeconds(secondsToCache),
                };

                this.traceLogger.LogDebug($"Adding token to claims cache for {secondsToCache} seconds (hash: {accessTokenHash})");
                await this.cache.SetAsync(accessTokenHash, bytes, options).ConfigureAwait(false);
            }
        }
    }
}
