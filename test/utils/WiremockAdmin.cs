namespace SampleApi.Test.Utils
{
    using System;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    /*
     * Manage updates to Wiremock
     */
    public class WiremockAdmin : IDisposable
    {
        private readonly WireMockServer server;

        public WiremockAdmin()
        {
            this.server = WireMockServer.Start(446);
        }

        /*
         * Register our test JWKS values at the start of the test suite
         */
        public void RegisterJsonWebWeys(string keysJson)
        {
            this.server
                .Given(Request.Create().WithPath("/.well-known/jwks.json").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200).WithBody(keysJson));
        }

        /*
         * Register a user at the start of a test
         */
        public void RegisterUserInfo(string userJson)
        {
            this.server
                .Given(Request.Create().WithPath("/oauth2/userInfo").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200).WithBody(userJson));
        }

        /*
         * Stop the server
         */
        public void Dispose()
        {
            this.server.Stop();
        }
    }
}
