namespace api.Plumbing
{
    using System.Net;
    using System.Net.Http;
    using api.Configuration;

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
