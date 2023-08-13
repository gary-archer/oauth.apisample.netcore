namespace SampleApi.IntegrationTests
{
    using System;
    using SampleApi.Test.Utils;

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
            this.WiremockAdmin = new WiremockAdmin(false);

            // Create the token issuer for these tests and issue some mock token signing keys
            this.TokenIssuer = new TokenIssuer();
            var keyset = this.TokenIssuer.GetTokenSigningPublicKeys();
            this.WiremockAdmin.RegisterJsonWebWeys(keyset).Wait();

            // Create the API client
            var apiBaseUrl = "https://apilocal.authsamples-dev.com:3446";
            var sessionId = Guid.NewGuid().ToString();
            this.ApiClient = new ApiClient(apiBaseUrl, "IntegrationTests", sessionId);
        }

        // A class to issue our own JWTs for testing
        public TokenIssuer TokenIssuer { get; private set; }

        // Wiremock will act as the authorization server and return canned OAuth responses
        public WiremockAdmin WiremockAdmin { get; private set; }

        // API client details
        public ApiClient ApiClient { get; private set; }

        /*
         * Destroy infrastructure resources when the test run ends
         */
        public void Dispose()
        {
            this.TokenIssuer.Dispose();
            this.WiremockAdmin.UnregisterJsonWebWeys().Wait();
        }
    }
}
