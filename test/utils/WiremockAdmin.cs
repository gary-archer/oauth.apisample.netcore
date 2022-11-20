namespace SampleApi.Test.Utils
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Utilities;

    /*
     * Manage updates to Wiremock
     */
    public class WiremockAdmin
    {
        private readonly string baseUrl;
        private readonly string jsonWebKeysId;
        private readonly string userInfoId;
        private readonly HttpProxy httpProxy;

        public WiremockAdmin(bool useProxy)
        {
            this.baseUrl = "https://login.authsamples-dev.com:447/__admin/mappings";
            this.jsonWebKeysId = Guid.NewGuid().ToString();
            this.userInfoId = Guid.NewGuid().ToString();
            this.httpProxy = new HttpProxy(useProxy, "http://127.0.0.1:8888");
        }

        /*
        * Register our test JWKS values at the start of the test suite
        */
        public async Task RegisterJsonWebWeys(string keysJson)
        {
            dynamic data = new JObject();
            data.id = this.jsonWebKeysId;
            data.priority = 1;

            dynamic request = new JObject();
            request.method = "GET";
            request.url = "/.well-known/jwks.json";
            data.request = request;

            dynamic response = new JObject();
            response.status = 200;
            response.body = keysJson;
            data.response = response;

            await this.Register(data.ToString());
        }

        /*
        * Unregister our test JWKS values at the end of the test suite
        */
        public async Task UnregisterJsonWebWeys()
        {
            await this.Unregister(this.jsonWebKeysId);
        }

        /*
        * Register a user at the start of a test
        */
        public async Task RegisterUserInfo(string userJson)
        {
            dynamic data = new JObject();
            data.id = this.userInfoId;
            data.priority = 1;

            dynamic request = new JObject();
            request.method = "POST";
            request.url = "/oauth2/userInfo";
            data.request = request;

            dynamic response = new JObject();
            response.status = 200;
            response.body = userJson;
            data.response = response;

            await this.Register(data);
        }

        /*
        * Unregister a user at the end of a test
        */
        public async Task UnregisterUserInfo()
        {
            await this.Unregister(this.userInfoId);
        }

        /*
        * Add a stubbed response to Wiremock via its Admin API
        */
        private async Task Register(string stubbedResponse)
        {
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(HttpMethod.Post, this.baseUrl);
                request.Content = new StringContent(stubbedResponse);
                await client.SendAsync(request);
            }
        }

        /*
        * Delete a stubbed response from Wiremock via its Admin API
        */
        private async Task Unregister(string id)
        {
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{this.baseUrl}/{id}");
                await client.SendAsync(request);
            }
        }
    }
}
