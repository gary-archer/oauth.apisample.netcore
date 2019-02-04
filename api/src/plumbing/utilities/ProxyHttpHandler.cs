namespace BasicApi.Plumbing.Utilities
{
    using System.Net;
    using System.Net.Http;
    using BasicApi.Configuration;

    /*
     * An HTTP handler that enables us to view requests from the API in an HTTP debugger
     */
    public class ProxyHttpHandler : HttpClientHandler
    {
        /*
         * Construct an HTTP client handler with a proxy if needed
         */
        public ProxyHttpHandler(ApplicationConfiguration config)
        {
            this.UseProxy = config.useProxy;
            if (config.useProxy)
            {
                this.Proxy = new WebProxy(config.ProxyUrl);
            }
        }
    }
}
