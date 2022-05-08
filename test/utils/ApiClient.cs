namespace SampleApi.Test.Utils
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Utilities;

    /*
     * A utility class to call the API in a parameterized manner
     */
    public sealed class ApiClient
    {
        private readonly string baseUrl;
        private readonly string clientName;
        private readonly string sessionId;
        private readonly HttpProxy httpProxy;

        public ApiClient(string baseUrl, string clientName, string sessionId, bool useProxy = false)
        {
            this.baseUrl = baseUrl;
            this.clientName = clientName;
            this.sessionId = sessionId;
            this.httpProxy = new HttpProxy(useProxy, "http://127.0.0.1:8888");
        }

        public async Task<ApiResponse> GetUserInfoClaims(ApiRequestOptions options)
        {
            options.HttpMethod = HttpMethod.Get;
            options.ApiPath = "/api/userinfo";
            return await this.CallApi(options);
        }

        public async Task<ApiResponse> GetCompanies(ApiRequestOptions options)
        {
            options.HttpMethod = HttpMethod.Get;
            options.ApiPath = "/api/companies";
            return await this.CallApi(options);
        }

        public async Task<ApiResponse> GetCompanyTransactions(ApiRequestOptions options, int companyId)
        {
            options.HttpMethod = HttpMethod.Get;
            options.ApiPath = $"/api/companies/{companyId}/transactions";
            return await this.CallApi(options);
        }

        private async Task<ApiResponse> CallApi(ApiRequestOptions options)
        {
            var url = $"{this.baseUrl}{options.ApiPath}";
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                // Create the request
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(options.HttpMethod, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);

                // Add custom headers for advanced testing if required
                if (options.RehearseException)
                {
                    request.Headers.Add("x-mycompany-test-exception", "SampleApi");
                }

                // Send the request
                var response = await client.SendAsync(request);

                // Return the status and body
                var body = await response.Content.ReadAsStringAsync();
                return new ApiResponse(response.StatusCode, body);
            }
        }
    }
}
