namespace SampleApi.Test.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Jose;
    using SampleApi.Plumbing.Utilities;

    /*
     * A mock authorization server implemented with wiremock and a JOSE library
     */
    public class MockAuthorizationServer : IDisposable
    {
        private readonly string adminBaseUrl;
        private readonly HttpProxy httpProxy;
        private readonly RSA rsa;
        private readonly Jwk tokenSigningPrivateKey;
        private readonly Jwk tokenSigningPublicKey;
        private readonly string keyId;

        public MockAuthorizationServer(bool useProxy = false)
        {
            this.adminBaseUrl = "https://login.authsamples-dev.com:447/__admin/mappings";
            this.httpProxy = new HttpProxy(useProxy, "http://127.0.0.1:8888");

            var algorithm = "RS256";
            this.rsa = RSA.Create(2048);
            this.keyId = Guid.NewGuid().ToString();

            this.tokenSigningPrivateKey = new Jwk(this.rsa, true);
            this.tokenSigningPrivateKey.Alg = algorithm;
            this.tokenSigningPrivateKey.KeyId = this.keyId;

            this.tokenSigningPublicKey = new Jwk(this.rsa, false);
            this.tokenSigningPublicKey.Alg = algorithm;
            this.tokenSigningPublicKey.KeyId = this.keyId;
        }

        public void Start()
        {
            var keyset = this.GetTokenSigningPublicKeys();
            this.RegisterJsonWebWeys(keyset).Wait();
        }

        public void Stop()
        {
            this.UnregisterJsonWebWeys().Wait();
        }

        /*
         * Get the token signing public keys as a JSON Web Keyset
         */
        public string GetTokenSigningPublicKeys()
        {
            var keyset = new JwkSet(this.tokenSigningPublicKey);
            return keyset.ToJson(JWT.DefaultSettings.JsonMapper);
        }

        /*
         * Issue an access token with the supplied subject claim
         */
        public string IssueAccessToken(MockTokenOptions options, Jwk jwk = null)
        {
            var now = DateTimeOffset.Now;
            var exp = now.AddMinutes(options.ExpiryMinutes);

            var headers = new Dictionary<string, object>()
            {
                { "kid", this.keyId },
            };

            var payload = new Dictionary<string, object>()
            {
                { "iss", options.Issuer },
                { "aud", options.Audience },
                { "scope", options.Scope },
                { "sub", options.Subject },
                { "manager_id", options.ManagerId },
                { "role", options.Role },
                { "exp", exp.ToUnixTimeSeconds() },
            };

            var jwkToUse = jwk ?? this.tokenSigningPrivateKey;
            return JWT.Encode(payload, jwkToUse, JwsAlgorithm.RS256, headers);
        }

        /*
         * Dispose the internal key
         */
        public void Dispose()
        {
            this.rsa.Dispose();
        }

        /*
         * Register our test JWKS values at the start of the test suite
         */
        private async Task RegisterJsonWebWeys(string keysJson)
        {
            var data = new JsonObject
            {
                ["Guid"] = this.keyId,
                ["Priority"] = 1,
                ["Request"] = new JsonObject
                {
                    ["Path"] = "/.well-known/jwks.json",
                    ["Methods"] = new JsonArray("get"),
                },
                ["Response"] = new JsonObject
                {
                    ["StatusCode"] = 200,
                    ["Body"] = keysJson,
                },
            };

            await this.Register(data.ToJsonString());
        }

        /*
         * Unregister our test JWKS values at the end of the test suite
         */
        private async Task UnregisterJsonWebWeys()
        {
            await this.Unregister(this.keyId);
        }

        /*
         * Add a stubbed response to Wiremock via its Admin API
         */
        private async Task Register(string stubbedResponse)
        {
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(HttpMethod.Post, this.adminBaseUrl);
                request.Content = new StringContent(stubbedResponse, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var status = (int)response.StatusCode;
                    var text = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Register call failed: {status}");
                }
            }
        }

        /*
         * Delete a stubbed response from Wiremock via its Admin API
         */
        private async Task Unregister(string id)
        {
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{this.adminBaseUrl}/{id}");
                await client.SendAsync(request);
            }
        }
    }
}
