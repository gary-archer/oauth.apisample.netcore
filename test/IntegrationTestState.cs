namespace FinalApi.IntegrationTests
{
    using System;
    using FinalApi.Test.Utils;

    /*
     * Contains state that is created once for the whole suite of integration tests
     */
    public class IntegrationTestState : IDisposable
    {
        /*
         * Create infrastructure resources once, when the test run begins
         */
        public IntegrationTestState()
        {
            // Create the mock authorization server, which enables productive API tests
            var useProxy = false;
            this.MockAuthorizationServer = new MockAuthorizationServer(useProxy);
            this.MockAuthorizationServer.Start();

            // Create the API client
            var apiBaseUrl = "https://api.authsamples-dev.com:446";
            var sessionId = Guid.NewGuid().ToString();
            this.ApiClient = new ApiClient(apiBaseUrl, "IntegrationTests", sessionId, useProxy);
        }

        // Wiremock and a JOSE library act as the mock authorization server
        public MockAuthorizationServer MockAuthorizationServer { get; private set; }

        // A wrapper for the API client
        public ApiClient ApiClient { get; private set; }

        /*
         * Destroy infrastructure resources when the test run ends
         */
        public void Dispose()
        {
            this.MockAuthorizationServer.Stop();
            this.MockAuthorizationServer.Dispose();
        }
    }
}
