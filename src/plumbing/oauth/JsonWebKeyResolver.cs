namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
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
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                var response = await client.GetJsonWebKeySetAsync(this.configuration.JwksEndpoint);
                if (response.IsError)
                {
                    if (response.Exception != null)
                    {
                        throw ErrorUtils.FromTokenSigningKeysDownloadError(response.Exception, this.configuration.JwksEndpoint);
                    }
                    else
                    {
                        throw ErrorUtils.FromTokenSigningKeysDownloadError(response, this.configuration.JwksEndpoint);
                    }
                }

                this.keyset = new JsonWebKeySet(response.Json.ToString());
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
