namespace FinalApi.Plumbing.Claims
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /*
     * A singleton memory cache for extra authorization values
     */
    public sealed class ClaimsCache
    {
        private readonly IDistributedCache cache;
        private readonly int timeToLiveMinutes;
        private readonly ILogger debugLogger;

        public ClaimsCache(
            IDistributedCache cache,
            int timeToLiveMinutes,
            ServiceProvider container)
        {
            this.cache = cache;
            this.timeToLiveMinutes = timeToLiveMinutes;
            this.debugLogger = container.GetService<ILoggerFactory>().CreateLogger<ClaimsCache>();
        }

        /*
         * Add an item to the cache and do not exceed the token's expiry or the configured time to live
         */
        public async Task SetItemAsync(string accessTokenHash, ExtraClaims claims, int expiry)
        {
            var now = DateTimeOffset.UtcNow;
            var epochSeconds = now.ToUnixTimeSeconds();
            var secondsToCache = expiry - epochSeconds;
            if (secondsToCache > 0)
            {
                if (secondsToCache > this.timeToLiveMinutes * 60)
                {
                    secondsToCache = this.timeToLiveMinutes * 60;
                }

                var json = JsonSerializer.Serialize(claims);
                var bytes = Encoding.UTF8.GetBytes(json);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = now.AddSeconds(secondsToCache),
                };

                this.debugLogger.LogDebug($"Adding item to cache for {secondsToCache} seconds (hash: {accessTokenHash})");
                await this.cache.SetAsync(accessTokenHash, bytes, options);
            }
        }

        /*
         * Read an item from the cache or return null if not found
         */
        public async Task<ExtraClaims> GetItemAsync(string accessTokenHash)
        {
            var bytes = await this.cache.GetAsync(accessTokenHash);
            if (bytes == null)
            {
                return null;
            }

            this.debugLogger.LogDebug($"Found existing item in cache (hash: {accessTokenHash})");
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<ExtraClaims>(json);
        }
    }
}
