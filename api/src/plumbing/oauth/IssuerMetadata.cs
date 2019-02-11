namespace BasicApi.Plumbing.OAuth
{
    using System;
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
        private readonly Func<ProxyHttpHandler> proxyFactory;
        private DiscoveryResponse metadata;

        /*
         * Receive dependencies
         */
        public IssuerMetadata(OAuthConfiguration configuration, Func<ProxyHttpHandler> proxyFactory)
        {
            this.configuration = configuration;
            this.proxyFactory = proxyFactory;
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
         * Load metadata from our configuration URL
         */
        public async Task Load()
        {
            using (var client = new DiscoveryClient(this.configuration.Authority, this.proxyFactory()))
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