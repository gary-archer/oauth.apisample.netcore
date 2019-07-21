namespace Framework.Utilities
{
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// An HTTP handler that enables us to view requests from the API in an HTTP debugger
    /// </summary>
    public sealed class ProxyHttpHandler : HttpClientHandler
    {
        /// <summary>
        /// Construct an HTTP client handler with a proxy if needed
        /// </summary>
        /// <param name="enabled">True if enabled</param>
        /// <param name="proxyUrl">The proxy URL if enabled</param>
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
