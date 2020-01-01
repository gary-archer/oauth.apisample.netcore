namespace Framework.OAuth
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Framework.Configuration;
    using Framework.Errors;
    using IdentityModel.Client;

    /*
     * A class to download Open Id Connect metadata at application startup
     */
    public sealed class IssuerMetadata
    {
        private readonly OAuthConfiguration configuration;
        private readonly Func<HttpClientHandler> proxyFactory;

        // The metadata
        public DiscoveryDocumentResponse Metadata { get; private set; }

        public IssuerMetadata(OAuthConfiguration configuration, Func<HttpClientHandler> proxyFactory)
        {
            this.configuration = configuration;
            this.proxyFactory = proxyFactory;
            this.Metadata = null;
        }

        /*
         * Load metadata from our configuration URL
         */
        public async Task Load()
        {
            using(var client = new HttpClient(this.proxyFactory()))
            {
                // Send the request
                var request = new DiscoveryDocumentRequest
                {
                    Address = this.configuration.Authority,
                    Policy = new DiscoveryPolicy()
                    {
                        // In my Okta account the following endpoint does not exist under a /default path segment
                        // This causes Identity Model endpoint validation to fail, do disable it here
                        // https://dev-843469.oktapreview.com/oauth2/v1/clients
                        ValidateEndpoints = false
                    }
                };
                var response = await client.GetDiscoveryDocumentAsync(request);

                // Handle errors
                if(response.IsError)
                {
                    var handler = new OAuthErrorHandler();
                    throw handler.FromMetadataError(response, this.configuration.Authority);
                }

                // Store the metadata
                this.Metadata = response;
            }
        }
    }
}