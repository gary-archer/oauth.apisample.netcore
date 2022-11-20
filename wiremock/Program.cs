using System;
using WireMock.Net.StandAlone;
using WireMock.Settings;

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
            }
        };

        StandAloneApp.Start(settings);
        Console.WriteLine("Wiremock is listening on port 447 ...");
        Console.ReadKey();
    }
}
