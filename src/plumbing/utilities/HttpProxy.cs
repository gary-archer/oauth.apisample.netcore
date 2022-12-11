namespace SampleApi.Plumbing.Utilities
{
    using System.Net;
    using System.Net.Http;

    /*
     * A utility to manage viewing OAuth messages in an HTTP proxy
     */
    public sealed class HttpProxy
    {
        private readonly bool isEnabled;
        private readonly string url;

        public HttpProxy(bool enabled, string proxyUrl)
        {
            this.isEnabled = enabled;
            this.url = proxyUrl;
        }

        /*
         * Return an HTTP client handler that supports proxying
         */
        public HttpClientHandler GetHandler()
        {
            var handler = new HttpClientHandler();
            if (this.isEnabled)
            {
                handler.Proxy = new WebProxy(this.url);
            }

            return handler;
        }
    }
}
