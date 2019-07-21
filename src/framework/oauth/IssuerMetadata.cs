namespace Framework.OAuth
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Framework.Configuration;
    using Framework.Errors;
    using IdentityModel.Client;

    /// <summary>
    /// A class to download Open Id Connect metadata at application startup
    /// </summary>
    public sealed class IssuerMetadata
    {
        // Dependencies
        private readonly OAuthConfiguration configuration;
        private readonly Func<HttpClientHandler> proxyFactory;

        // The metadata
        public DiscoveryResponse Metadata { get; private set; }

        /// <summary>
        /// Receive dependencies
        /// </summary>
        /// <param name="configuration">The OAuth configuration</param>
        /// <param name="proxyFactory">An object from which to create HTTP proxy handlers</param>
        public IssuerMetadata(OAuthConfiguration configuration, Func<HttpClientHandler> proxyFactory)
        {
            this.configuration = configuration;
            this.proxyFactory = proxyFactory;
            this.Metadata = null;
        }

        /// <summary>
        /// Load metadata from our configuration URL
        /// </summary>
        /// <returns>A task to await</returns>
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
                    var handler = new OAuthErrorHandler();
                    throw handler.FromMetadataError(response, this.configuration.Authority);
                }

                // Return the user info endpoint
                this.Metadata = response;
            }
        }
    }
}