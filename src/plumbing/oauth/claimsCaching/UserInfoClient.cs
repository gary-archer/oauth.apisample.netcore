namespace SampleApi.Plumbing.OAuth.ClaimsCaching
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Utilities;

    /*
     * A class to lookup extra claims from the User Info endpoint
     */
    public sealed class UserInfoClient
    {
        private readonly OAuthConfiguration configuration;
        private readonly HttpProxy httpProxy;
        private readonly LogEntry logEntry;

        public UserInfoClient(
            OAuthConfiguration configuration,
            HttpProxy httpProxy,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.httpProxy = httpProxy;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * The sample API performs OAuth user info lookup when using the apiLookup claims strategy
         */
        public async Task<IEnumerable<Claim>> GetUserInfoAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
                try
                {
                    using (var client = new HttpClient(this.httpProxy.GetHandler()))
                    {
                        // Send the request
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var request = new HttpRequestMessage(HttpMethod.Get, this.configuration.ClaimsCache.UserInfoEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        var response = await client.SendAsync(request);

                        // Report errors with a response
                        if (!response.IsSuccessStatusCode)
                        {
                            var status = (int)response.StatusCode;
                            var text = await response.Content.ReadAsStringAsync();
                            throw ErrorUtils.FromUserInfoError(status, text, this.configuration.ClaimsCache.UserInfoEndpoint);
                        }

                        // Convert from JSON data to an array of claims
                        var json = await response.Content.ReadAsStringAsync();
                        return ClaimsReader.UserInfoClaims(json);
                    }
                }
                catch (Exception ex)
                {
                    // Report connectity errors
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.ClaimsCache.UserInfoEndpoint);
                }
            }
        }
    }
}
