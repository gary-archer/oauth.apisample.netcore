namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;
    using BasicApi.Entities;

    /*
     * Encapsulate getting and setting claims from the cache
     */
    public class ClaimsCache
    {
        private readonly IDistributedCache cache;

        /*
         * Receive the Microsoft cache object that we are wrapping
         */
        public ClaimsCache(IDistributedCache cache)
        {
            this.cache = cache;
        }

        /*
         * Add our custom claims to the cache
         */
        public async Task AddClaimsForTokenAsync(string accessToken, int expiry, ApiClaims claims)
        {
            // Get the hash as a cache key
            var hash = Sha256(accessToken);

            // Serialize to bytes
            var json = JsonConvert.SerializeObject(claims);
            var bytes = Encoding.UTF8.GetBytes(json);

            // Calculate expiry
            var expiryTime = EpochTimeToDateTimeOffset(expiry);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiryTime
            };

            // Add to the cache
            // await this.cache.SetAsync(hash, bytes, options).ConfigureAwait(false);
        }

        /*
         * Read our custom claims from the cache or return null if not found
         */
        public async Task<bool> GetClaimsForTokenAsync(string accessToken, ApiClaims claims)
        {
            // Get the hash as a cache key
            var hash = Sha256(accessToken);

            // See if bytes exists in the cache
            // var bytes = await this.cache.GetAsync(hash).ConfigureAwait(false);
            // if (bytes == null)
            {
                return false;
            }

            // Deserialize to claims
            // var json = Encoding.UTF8.GetString(bytes);
            // JsonConvert.DeserializeObject<ApiClaims>(json);
            // return true;

            // TODO: Update supplied object
        }

        /*
         * Get the hash of input
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

        /*
         * Convert the expiry time from the token to the format expected by the cache
         */
        private static DateTimeOffset EpochTimeToDateTimeOffset(int input)
        {
            var timeInTicks = input * TimeSpan.TicksPerSecond;
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddTicks(timeInTicks);
        }
    }
}
