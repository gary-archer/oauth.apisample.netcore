namespace FinalApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Jose;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Errors;
    using FinalApi.Plumbing.Utilities;

    /*
    * Do the signing key download without needing to plug a website library into an API
    */
    public sealed class JsonWebKeyResolver
    {
        private readonly OAuthConfiguration configuration;
        private readonly JwksCache cache;
        private readonly HttpProxy httpProxy;

        public JsonWebKeyResolver(OAuthConfiguration configuration, JwksCache cache, HttpProxy httpProxy)
        {
            this.configuration = configuration;
            this.cache = cache;
            this.httpProxy = httpProxy;
        }

        /*
         * Return cached keys or download if a new kid is received
         */
        public async Task<Jwk> GetKeyForId(string kid)
        {
            try
            {
                // Try to load keys from the cache
                var cachedJson = await this.cache.GetJwksKeysAsync();
                if (cachedJson != null)
                {
                    var cachedKeySet = JwkSet.FromJson(cachedJson, JWT.DefaultSettings.JsonMapper);
                    var foundInCache = cachedKeySet.Keys.FirstOrDefault(k => k.KeyId == kid);
                    if (foundInCache != null)
                    {
                        return foundInCache;
                    }
                }

                // If not found then do a new download
                var json = await this.DownloadKeys();
                var keyset = JwkSet.FromJson(json, JWT.DefaultSettings.JsonMapper);
                var found = keyset.Keys.FirstOrDefault(k => k.KeyId == kid);

                // If found then update the cache
                if (found != null)
                {
                    await this.cache.SetJwksKeysAsync(json);
                    return found;
                }

                // Indicate not found
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromTokenSigningKeysDownloadError(ex, this.configuration.JwksEndpoint);
            }
        }

        /*
         * Do the download when a new kid is received
         */
        private async Task<string> DownloadKeys()
        {
            try
            {
                using (var client = new HttpClient(this.httpProxy.GetHandler()))
                {
                    // Send the request
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var request = new HttpRequestMessage(HttpMethod.Get, this.configuration.JwksEndpoint);
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var status = (int)response.StatusCode;
                        throw ErrorUtils.FromTokenSigningKeysDownloadError(status, this.configuration.JwksEndpoint);
                    }

                    // Get and cache results
                    var json = await response.Content.ReadAsStringAsync();
                    await this.cache.SetJwksKeysAsync(json);
                    return json;
                }
            }
            catch (Exception ex)
            {
                // Report connectivity errors
                throw ErrorUtils.FromTokenSigningKeysDownloadError(ex, this.configuration.JwksEndpoint);
            }
        }
    }
}
