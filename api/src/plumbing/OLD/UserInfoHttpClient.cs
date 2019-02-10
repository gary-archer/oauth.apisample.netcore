namespace BasicApi.Plumbing.OAuth
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.Extensions.Configuration;
    using BasicApi.Configuration;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.OAuth;
    using BasicApi.Plumbing.Utilities;

    /*
     * A wrapper class to manage looking up central user info
     */
    public class UserInfoHttpClient
    {
        
        /*
         * Store configuration
         */
        private readonly OAuthConfiguration oauthConfig;
        private readonly ApplicationConfiguration appConfig;

        /*
         * Construct from configuration
         */
        public UserInfoHttpClient(IConfiguration configuration)
        {
            this.oauthConfig = null; //OAuthConfiguration.Load(configuration);
            this.appConfig = null; //ApplicationConfiguration.Load(configuration);
        }

        /*
         * This sample uses Okta user info as the source of central user data
         * Since getting user info is an OAuth operation we include that in this class also
         */
        public async Task LookupCentralUserDataClaimsAsync(ApiClaims claims, string accessToken)
        {
            // Do the downloads
            var userInfoEndpoint = await this.GetUserInfoEndpointAsync();
            var userInfo = await this.GetUserInfoAsync(userInfoEndpoint, accessToken);

            // Get  claims
            string givenName = userInfo.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            string familyName = userInfo.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            string email = userInfo.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            // Set our user info data points
            claims.SetCentralUserInfo(givenName, familyName, email);
        }

        /*
         * Download metadata to get the user info endpoint
         */
        private async Task<string> GetUserInfoEndpointAsync()
        {
            using (var client = new DiscoveryClient(this.oauthConfig.Authority, new ProxyHttpHandler(this.appConfig)))
            {
                // Our Okta developer setup requires this but we wouldn't use it for a production solution
                client.Policy = new DiscoveryPolicy()
                {
                    ValidateEndpoints = false
                };
                
                // Note that the IdentityModel calls ConfigureAwait(false) on the client
                DiscoveryResponse response = await client.GetAsync();
                
                // Handle errors
                if (this.IsFailedResponse(response.IsError, response.StatusCode))
                {
                    var handler = new ErrorHandler();
                    throw handler.FromMetadataError(response, this.oauthConfig.Authority);
                }

                // Return the user info endpoint
                return response.UserInfoEndpoint;
            }
        }

        /*
         * We download central user info from the Authorization Server
         */
        private async Task<UserInfoResponse> GetUserInfoAsync(string userInfoEndpoint, string accessToken)
        {
            using (var client = new UserInfoClient(userInfoEndpoint, new ProxyHttpHandler(this.appConfig)))
            {
                // Note that the IdentityModel calls ConfigureAwait(false) on the client
                UserInfoResponse response = await client.GetAsync(accessToken );

                // Handle errors
                if (this.IsFailedResponse(response.IsError, response.HttpStatusCode))
                {
                    var handler = new ErrorHandler();
                    throw handler.FromUserInfoError(response, userInfoEndpoint);
                }

                // Return the user info data
                return response;
            }
        }

        /*
         * Some messages can return a 401 with an empty response body so use the below logic
         */
        private bool IsFailedResponse(bool isError, HttpStatusCode responseStatusCode)
        {
            return isError || (int)responseStatusCode >= 400;
        }
    }
}
