namespace Framework.OAuth
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;
    using Framework.Configuration;

    /// <summary>
    /// Encapsulate getting and setting claims from the cache
    /// </summary>
    public sealed class ClaimsCache<TClaims> where TClaims: CoreApiClaims
    {
        private readonly IDistributedCache cache;
        private readonly OAuthConfiguration configuration;

        /// <summary>
        /// Receive the Microsoft cache object that we are wrapping
        /// </summary>
        /// <param name="cache">The Microsoft thread safe cache</param>
        /// <param name="configuration">Our configuration</param>
        public ClaimsCache(IDistributedCache cache, OAuthConfiguration configuration)
        {
            this.cache = cache;
            this.configuration = configuration;
        }

        /// <summary>
        /// Add our custom claims to the cache
        /// </summary>
        /// <param name="accessToken">The access token</param>
        /// <param name="expiryClaimSeconds">The expiry time of the token as a number of seconds since the Unix Epoch time</param>
        /// <param name="claims">Our claims as a plain .Net object</param>
        /// <returns>A task to await</returns>
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

        /// <summary>
        /// Read our custom claims from the cache or return null if not found
        /// </summary>
        /// <param name="accessToken">The access token</param>
        /// <returns>The claims or null if not found</returns>
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

        /// <summary>
        /// Get the hash of an input string
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>A SHA 256 hash of the input</returns>
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
