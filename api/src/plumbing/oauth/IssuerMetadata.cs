namespace BasicApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using BasicApi.Configuration;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.Utilities;
    using IdentityModel.Client;

    /*
     * A class to download Open Id Connect metadata at application startup
     */
    public class IssuerMetadata
    {
        private readonly OAuthConfiguration configuration;
        private readonly ProxyHttpHandler proxyHandler;
        private DiscoveryResponse metadata;

        /*
         * Receive dependencies
         */
        public IssuerMetadata(OAuthConfiguration configuration, ProxyHttpHandler proxyHandler)
        {
            this.configuration = configuration;
            this.proxyHandler = proxyHandler;
            this.metadata = null;
        }

        /*
         * Return the metadata once loaded
         */
        public DiscoveryResponse Metadata
        {
            get
            {
                return this.metadata;
            }
        }

        /*
         * Return the proxy handler to consumers of the metadata who want to make HTTP calls
         */
        public ProxyHttpHandler ProxyHandler
        {
            get
            {
                return this.proxyHandler;
            }
        }

        /* 
         * Load metadata from our configuration URL
         */
        public async Task Load()
        {
            using (var client = new DiscoveryClient(this.configuration.Authority, this.proxyHandler))
            {
                // In my Okta account the following endpoint does not exist under a /default path segment
                // This causes Identity Model endpoint validation to fail, do disable it here
                // https://dev-843469.oktapreview.com/oauth2/v1/clients
                client.Policy = new DiscoveryPolicy()
                {
                    ValidateEndpoints = false
                };
                
                // Make the request
                DiscoveryResponse response = await client.GetAsync();
                
                // Handle errors
                if(response.IsError)
                {
                    var handler = new ErrorHandler();
                    throw handler.FromMetadataError(response, this.configuration.Authority);
                }

                // Return the user info endpoint
                this.metadata = response;
            }
        }
    }
}