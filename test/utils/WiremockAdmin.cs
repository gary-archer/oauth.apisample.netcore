namespace SampleApi.Test.Utils
{
    using System;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;
    using WireMock.Settings;

    /*
     * Manage updates to Wiremock
     */
    public class WiremockAdmin : IDisposable
    {
        private readonly WireMockServer server;

        public WiremockAdmin()
        {
            var settings = new WireMockServerSettings
            {
                Port = 446,
                UseSSL = true,
                CertificateSettings = new WireMockCertificateSettings
                {
                    X509CertificateFilePath = "../../../../certs/authsamples-dev.ssl.p12",
                    X509CertificatePassword = "Password1",
                },
            };
            this.server = WireMockServer.Start(settings);
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
