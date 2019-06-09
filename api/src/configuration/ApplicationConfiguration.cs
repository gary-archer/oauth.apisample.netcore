namespace BasicApi.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Application specific settings
    /// </summary>
    public class ApplicationConfiguration
    {
        public List<string> TrustedOrigins {get; set;}

        public string SslCertificateFileName {get; set;}

        public string SslCertificatePassword {get; set;}

        public string ProxyUrl {get; set;}

        public bool useProxy {get; set;}
    }
}
