using System;
using WireMock.Net.StandAlone;
using WireMock.Settings;
using WireMock.Logging;

class Program
{
    static void Main(string[] args)
    {
        var settings = new WireMockServerSettings
        {
            Urls = new [] { "https://login.authsamples-dev.com:447" },
            CertificateSettings = new WireMockCertificateSettings
            {
                X509CertificateFilePath = "../certs/authsamples-dev.ssl.p12",
                X509CertificatePassword = "Password1"
            },
            StartAdminInterface = true,
            AllowPartialMapping = true,
            /* Logger = new WireMockConsoleLogger() */
        };

        StandAloneApp.Start(settings);
        Console.WriteLine("Wiremock is listening on port 447 ...");
        Console.ReadKey();
    }
}
