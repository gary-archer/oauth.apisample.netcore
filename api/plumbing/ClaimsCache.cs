namespace api.Plumbing
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;
    using api.Entities;

    /*
     * Encapsulate cache getting and setting
     */
    public static class ClaimsCache
    {
        /*
         * Add our custom claims to the cache
         */
        public static async Task AddClaimsMappedToTokenAsync(
            this IDistributedCache cache, 
            string token, 
            int expiry, 
            ApiClaims claims,
            ILogger logger)
        {
            // Get the hash as a cache key
            var hash = Sha256($"CLAIMS_${token}");

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
            logger.LogInformation($"ClaimsCache: Caching claims for token hash {hash} until {expiryTime}");
            await cache.SetAsync(hash, bytes, options).ConfigureAwait(false);
        }

        /*
         * Read our custom claims from the cache or return null if not found
         */
        public static async Task<ApiClaims> GetClaimsMappedToTokenAsync(
            this IDistributedCache cache,
            string token,
            ILogger logger)
        {
            // Get the hash as a cache key
            var hash = Sha256($"CLAIMS_${token}");

            // See if bytes exists in the cache
            var bytes = await cache.GetAsync(hash).ConfigureAwait(false);
            if (bytes == null)
            {
                logger.LogInformation($"ClaimsCache: No existing claims found for token hash {hash}");
                return null;
            }

            // Deserialize to claims
            logger.LogInformation($"ClaimsCache: Found existing claims for token hash {hash}");
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ApiClaims>(json);
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
