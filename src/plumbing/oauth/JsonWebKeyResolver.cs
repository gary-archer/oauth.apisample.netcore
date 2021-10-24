namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Utilities;

    /*
    * Do the signing key download without needing to plug a website library into an API
    */
    internal sealed class JsonWebKeyResolver
    {
        private readonly OAuthConfiguration configuration;
        private readonly HttpProxy httpProxy;
        private JsonWebKeySet keyset;

        public JsonWebKeyResolver(OAuthConfiguration configuration, HttpProxy httpProxy)
        {
            this.configuration = configuration;
            this.httpProxy = httpProxy;
            this.keyset = null;
        }

        /*
         * Return cached keys or download if a new kid is received
         */
        public async Task<JsonWebKey> GetKeyForId(string kid)
        {
            try
            {
                var found = this.FindKey(kid);
                if (found != null)
                {
                    return found;
                }

                await this.DownloadKeys();

                found = this.FindKey(kid);
                if (found != null)
                {
                    return found;
                }

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
        private async Task DownloadKeys()
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
                        throw ErrorUtils.FromTokenSigningKeysDownloadError(response, this.configuration.JwksEndpoint);
                    }

                    // Return results
                    var json = await response.Content.ReadAsStringAsync();
                    this.keyset = new JsonWebKeySet(json);
                }
            }
            catch (Exception ex)
            {
                // Report connectivity errors
                throw ErrorUtils.FromTokenSigningKeysDownloadError(ex, this.configuration.JwksEndpoint);
            }
        }

        /*
         * Find a key in the keyset from the key identifier received in a JWT
         */
        private JsonWebKey FindKey(string kid)
        {
            if (this.keyset == null)
            {
                return null;
            }

            return this.keyset.Keys.First(k => k.KeyId == kid);
        }
    }
}
