namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;

    /*
     * A class to download Open Id Connect metadata at application startup
     */
    internal sealed class IssuerMetadata
    {
        private readonly OAuthConfiguration configuration;
        private readonly Func<HttpClientHandler> proxyFactory;

        public IssuerMetadata(OAuthConfiguration configuration, Func<HttpClientHandler> proxyFactory)
        {
            this.configuration = configuration;
            this.proxyFactory = proxyFactory;
            this.Metadata = null;
        }

        // Return the metadata
        public DiscoveryDocumentResponse Metadata { get; private set; }

        /*
         * Load metadata from our configuration URL
         */
        public async Task Load()
        {
            using (var client = new HttpClient(this.proxyFactory()))
            {
                // Send the request
                var request = new DiscoveryDocumentRequest
                {
                    Address = this.configuration.Authority,
                    Policy = new DiscoveryPolicy()
                    {
                        // Prevent 'Endpoint is on a different host than authority' errors since Cognito uses different values
                        ValidateEndpoints = false,
                    },
                };
                var response = await client.GetDiscoveryDocumentAsync(request);

                // Handle errors
                if (response.IsError)
                {
                    throw ErrorUtils.FromMetadataError(response, this.configuration.Authority);
                }

                // Store the metadata
                this.Metadata = response;
            }
        }
    }
}