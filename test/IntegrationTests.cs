namespace SampleApi.Test.TokenIssuer
{
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using SampleApi.Test.Utils;

    /*
     * Test the API in isolation, without any dependencies on the Authorization Server
     */
    [TestFixture]
    public class IntegrationTests
    {
        // The real subject claim values for my two online test users
        private string guestUserId = "a6b404b1-98af-41a2-8e7f-e4061dc0bf86";
        private string guestAdminId = "77a97e5b-b748-45e5-bb6f-658e85b2df91";

        // A class to issue our own JWTs for testing
        private TokenIssuer tokenIssuer;
        private WiremockAdmin wiremockAdmin;

        // API client details
        private string apiBaseUrl;
        private ApiClient apiClient;

        /*
         * Initialize mock token issuing and wiremock
         */
        [OneTimeSetUp]
        public void Setup()
        {
            this.tokenIssuer = new TokenIssuer();
            this.wiremockAdmin = new WiremockAdmin();

            var keyset = this.tokenIssuer.GetTokenSigningPublicKeys();
            this.wiremockAdmin.RegisterJsonWebWeys(keyset);

            // Create the API client
            this.apiBaseUrl = "https://api.authsamples-dev.com:445";
            this.apiClient = new ApiClient(this.apiBaseUrl, false);
        }

        /*
         * Clean up resources after all tests have completed
         */
        [OneTimeTearDown]
        public void Teardown()
        {
            this.tokenIssuer.Dispose();
            this.wiremockAdmin.Dispose();
        }

        /*
         * Test getting claims
         */
        [Test]
        public async Task GetUserClaims_ReturnsSingleRegion_ForStandardUser()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestUserId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Guest";
            data.family_name = "User";
            data.email = "guestuser@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetUserInfoClaims(options);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unexpected HTTP status code");
            var claims = JObject.Parse(response.Body);
            var regions = claims.Value<JArray>("regions");
            Assert.AreEqual(1, regions.Count, "Unexpected regions claim");
        }

        /*
         * Test getting claims for the admin user
         */
        [Test]
        public async Task GetUserClaims_ReturnsAllRegions_ForAdminUser()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestAdminId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Admin";
            data.family_name = "User";
            data.email = "guestadmin@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetUserInfoClaims(options);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unexpected HTTP status code");
            var claims = JObject.Parse(response.Body);
            var regions = claims.Value<JArray>("regions");
            Assert.AreEqual(3, regions.Count, "Unexpected regions claim");
        }

        /*
         * Test getting companies
         */
        [Test]
        public async Task GetCompanies_ReturnsTwoItems_ForStandardUser()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestUserId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Guest";
            data.family_name = "User";
            data.email = "guestuser@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetCompanies(options);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unexpected HTTP status code");
            var companies = JArray.Parse(response.Body);
            Assert.AreEqual(2, companies.Count, "Unexpected companies list");
        }

        /*
         * Test getting companies for the admin user
         */
        [Test]
        public async Task GetCompanies_ReturnsAllItems_ForAdminUser()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestAdminId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Admin";
            data.family_name = "User";
            data.email = "guestadmin@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetCompanies(options);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unexpected HTTP status code");
            var companies = JArray.Parse(response.Body);
            Assert.AreEqual(4, companies.Count, "Unexpected companies list");
        }

        /*
         * Test getting companies with a malicious JWT access token
         */
        [Test]
        public async Task GetCompanies_Returns401_ForMaliciousJwt()
        {
            // Get a malicious access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueMaliciousAccessToken(this.guestUserId);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetCompanies(options);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.AreEqual("unauthorized", code, "Unexpected error code");
        }

        /*
         * Test getting allowed transactions
         */
        [Test]
        public async Task GetTransactions_ReturnsAllowedItems_ForCompaniesMatchingTheRegionClaim()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestUserId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Guest";
            data.family_name = "User";
            data.email = "guestuser@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetCompanyTransactions(options, 2);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unexpected HTTP status code");
            var payload = JObject.Parse(response.Body);
            var transactions = payload.Value<JArray>("transactions");
            Assert.AreEqual(8, transactions.Count, "Unexpected transactions list");
        }

        /*
         * Test getting unauthorized transactions
         */
        [Test]
        public async Task GetTransactions_ReturnsNotFoundForUser_ForCompaniesNotMatchingTheRegionClaim()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestUserId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Guest";
            data.family_name = "User";
            data.email = "guestuser@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.apiClient.GetCompanyTransactions(options, 3);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.AreEqual("company_not_found", code, "Unexpected error code");
        }

        /*
         * Test rehearsing a 500 error when there is an exception in the API
         */
        [Test]
        public async Task FailedApiCall_ReturnsSupportable500Error_ForErrorRehearsalRequest()
        {
            // Get an access token for the end user of this test
            var accessToken = this.tokenIssuer.IssueAccessToken(this.guestUserId);

            // The API will call the Authorization Server to get user info for the token, so register a mock response
            dynamic data = new JObject();
            data.given_name = "Guest";
            data.family_name = "User";
            data.email = "guestuser@mycompany.com";
            this.wiremockAdmin.RegisterUserInfo(data.ToString());

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            options.RehearseException = true;
            var response = await this.apiClient.GetCompanyTransactions(options, 3);

            // Assert expected results
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.AreEqual("exception_simulation", code, "Unexpected error code");
        }
    }
}