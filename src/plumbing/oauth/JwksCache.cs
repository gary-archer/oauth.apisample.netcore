namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;

    /*
     * A thread safe cache of JWKS keys, since jose-jwt does not support this
     */
    public sealed class JwksCache
    {
        private readonly IDistributedCache cache;
        private readonly int timeToLiveMinutes;

        public JwksCache(IDistributedCache cache)
        {
            this.cache = cache;
            this.timeToLiveMinutes = 60 * 12;
        }

        /*
         * Add JWKS keys to the cache
         */
        public async Task SetJwksKeysAsync(string json)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(this.timeToLiveMinutes * 60),
            };

            var bytes = Encoding.UTF8.GetBytes(json);
            await this.cache.SetAsync("JWKS", bytes, options);
        }

        /*
         * Get JWKS keys from the cache
         */
        public async Task<string> GetJwksKeysAsync()
        {
            var bytes = await this.cache.GetAsync("JWKS");
            if (bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
