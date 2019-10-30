namespace Framework.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;
    using Framework.Configuration;

    /*
     * Encapsulate getting and setting claims from the cache
     */
    public sealed class ClaimsCache<TClaims> where TClaims: CoreApiClaims
    {
        private readonly IDistributedCache cache;
        private readonly OAuthConfiguration configuration;

        public ClaimsCache(IDistributedCache cache, OAuthConfiguration configuration)
        {
            this.cache = cache;
            this.configuration = configuration;
        }

        /*
         * Add our custom claims to the cache
         */
        public async Task AddClaimsForTokenAsync(string accessToken, int expiryClaimSeconds, TClaims claims)
        {
            // Check for a race condition where the token passes validation but it expired when it gets here
            var utcNow = DateTimeOffset.UtcNow;
            var expiration = DateTimeOffset.UnixEpoch.AddSeconds(expiryClaimSeconds);
            if (expiration > utcNow)
            {
                // Calculate the max duration
                var maxDuration = DateTimeOffset.UtcNow.AddMinutes(this.configuration.DefaultTokenCacheMinutes);
                if (expiration > maxDuration)
                {
                    expiration = maxDuration;
                }
                
                // Serialize claims to bytes
                var json = JsonConvert.SerializeObject(claims);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Add to the cache
                var hash = Sha256(accessToken);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration
                };

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
                return null;
            }

            // Deserialize and return the cached data
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
