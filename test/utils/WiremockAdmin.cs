namespace SampleApi.Test.Utils
{
    using System.Threading.Tasks;

    /*
     * Manage updates to Wiremock
     */
    public class WiremockAdmin
    {
        public WiremockAdmin(bool useProxy)
        {
        }

        /*
         * Register our test JWKS values at the start of the test suite
         */
        public async Task RegisterJsonWebWeys(string keysJson)
        {
        }

        /*
         * Unregister our test JWKS values at the end of the test suite
         */
        public async Task UnregisterJsonWebWeys()
        {
        }

        /*
         * Register a user at the start of a test
         */
        public async Task RegisterUserInfo(string userJson)
        {
        }

        /*
         * Unregister a user at the end of a test
         */
        public async Task UnregisterUserInfo()
        {
        }
    }
}
