namespace SampleApi.Host.Plumbing.Utilities
{
    using System.Net;
    using System.Net.Http;

    /*
     * An HTTP handler that enables us to view requests from the API in an HTTP debugger
     */
    public sealed class ProxyHttpHandler : HttpClientHandler
    {
        /*
         * Construct an HTTP client handler with a proxy if needed
         */
        public ProxyHttpHandler(bool enabled, string proxyUrl)
        {
            this.UseProxy = enabled;
            if (enabled)
            {
                this.Proxy = new WebProxy(proxyUrl);
            }
        }
    }
}
