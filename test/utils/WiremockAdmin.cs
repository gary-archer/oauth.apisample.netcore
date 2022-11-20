namespace SampleApi.Test.Utils
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Utilities;

    /*
     * Manage updates to Wiremock via the Admin API
     * https://github.com/WireMock-Net/WireMock.Net/wiki/Admin-API-Reference
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
            data.Guid = this.jsonWebKeysId;
            data.Priority = 1;

            dynamic request = new JObject();
            request.Path = "/.well-known/jwks.json";

            dynamic methods = new JArray();
            methods.Add("get");
            request.Methods = methods;

            data.Request = request;

            dynamic response = new JObject();
            response.StatusCode = 200;
            response.Body = keysJson;
            data.Response = response;

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
            data.Guid = this.userInfoId;
            data.Priority = 1;

            dynamic request = new JObject();
            request.Path = "/oauth2/userInfo";

            dynamic methods = new JArray();
            methods.Add("post");
            request.Methods = methods;

            data.Request = request;

            dynamic response = new JObject();
            response.StatusCode = 200;
            response.Body = userJson;
            data.Response = response;

            await this.Register(data.ToString());
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
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{this.baseUrl}/{id}");
                await client.SendAsync(request);
            }
        }
    }
}
