namespace SampleApi.Test.Utils
{
    using System;
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
            options.ApiPath = "/investments/userinfo";

            var metrics = new ApiResponseMetrics("getUserInfoClaims");
            return await this.CallApi(options, metrics);
        }

        public async Task<ApiResponse> GetCompanies(ApiRequestOptions options)
        {
            options.HttpMethod = HttpMethod.Get;
            options.ApiPath = "/investments/companies";

            var metrics = new ApiResponseMetrics("getCompanies");
            return await this.CallApi(options, metrics);
        }

        public async Task<ApiResponse> GetCompanyTransactions(ApiRequestOptions options, int companyId)
        {
            options.HttpMethod = HttpMethod.Get;
            options.ApiPath = $"/investments/companies/{companyId}/transactions";

            var metrics = new ApiResponseMetrics("getCompanyTransactions");
            return await this.CallApi(options, metrics);
        }

        /*
         * Parameterized code to do the async work of calling the API
         */
        private async Task<ApiResponse> CallApi(ApiRequestOptions options, ApiResponseMetrics metrics)
        {
            // Initialize metrics
            var correlationId = Guid.NewGuid().ToString();
            metrics.StartTime = DateTime.UtcNow;
            metrics.CorrelationId = correlationId;

            // Prepare the request
            var url = $"{this.baseUrl}{options.ApiPath}";
            using (var client = new HttpClient(this.httpProxy.GetHandler()))
            {
                // Create the request
                var request = new HttpRequestMessage(options.HttpMethod, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);

                // Add headers
                request.Headers.Add("x-mycompany-api-client", this.clientName);
                request.Headers.Add("x-mycompany-session-id", this.sessionId);
                request.Headers.Add("x-mycompany-correlation-id", correlationId);
                if (options.RehearseException)
                {
                    request.Headers.Add("x-mycompany-test-exception", "SampleApi");
                }

                // Send the request
                var response = await client.SendAsync(request);

                // Record the time taken
                metrics.MillisecondsTaken = (int)(DateTime.UtcNow - metrics.StartTime).TotalMilliseconds;

                // Return response details
                var body = await response.Content.ReadAsStringAsync();
                return new ApiResponse(response.StatusCode, body, metrics);
            }
        }
    }
}
