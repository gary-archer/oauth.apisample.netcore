namespace BasicApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using BasicApi.Configuration;

    /*
     * A class to download Open Id Connect metadata at application startup
     */
    public class IssuerMetadata
    {
        private readonly OAuthConfiguration configuration;

        /*
         * Receive dependencies
         */
        public IssuerMetadata(OAuthConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /* 
         * Load metadata from our configuration URL
         */
        public async Task Load()
        {
            return;
        }
    }
}