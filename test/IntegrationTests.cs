namespace SampleApi.IntegrationTests
{
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Jose;
    using Newtonsoft.Json.Linq;
    using SampleApi.Test.Utils;
    using Xunit;

    /*
     * Test the API in isolation, without any dependencies on the Authorization Server
     */
    public class IntegrationTests : IClassFixture<IntegrationTestState>
    {
        // State shared across the suite of tests
        private readonly IntegrationTestState state;

        /*
         * Initialize mock token issuing and wiremock before a test runs
         */
        public IntegrationTests(IntegrationTestState state)
        {
            this.state = state;
        }

        /*
         * Test that a request without an access token is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns401_ForMissingJwt()
        {
            // Call the API and ensure a 401 response
            var options = new ApiRequestOptions(string.Empty);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "invalid_token", "Unexpected error code");
        }*/

        /*
         * Test that an expired access token is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns401_ForExpiredJwt()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            jwtOptions.ExpiryMinutes = -30;
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API and ensure a 401 response
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "invalid_token", "Unexpected error code");
        }*/

        /*
         * Test that an access token with an invalid issuer is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns401_ForInvalidIssuer()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            jwtOptions.Issuer = "https://otherissuer.com";
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API and ensure a 401 response
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "invalid_token", "Unexpected error code");
        }*/

        /*
         * Test that an access token with an invalid audience is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns401_ForInvalidAudience()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            jwtOptions.Audience = "api.other.com";
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API and ensure a 401 response
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "invalid_token", "Unexpected error code");
        }*/

        /*
         * Test that an access token with an invalid signature is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns401_ForInvalidSignature()
        {
            using (var rsa = RSA.Create(2048))
            {
                // Get an access token signed with a malicious token signing key
                var jwk = new Jwk(rsa, true);
                var jwtOptions = new MockTokenOptions();
                jwtOptions.UseStandardUser();
                var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions, jwk);

                // Call the API and ensure a 401 response
                var options = new ApiRequestOptions(accessToken);
                var response = await this.state.ApiClient.GetCompanies(options);

                // Assert expected results
                Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
                var error = JObject.Parse(response.Body);
                var code = error.Value<string>("code");
                Assert.True(code == "invalid_token", "Unexpected error code");
            }
        }*/

        /*
         * Test that an access token with an invalid scope is rejected
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_Returns403_ForInvalidScope()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            jwtOptions.Scope = "openid profile";
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API and ensure a 401 response
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "insufficient_scope", "Unexpected error code");
        }*/

        /*
         * Test rehearsing a 500 error when there is an exception in the API
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task CallApi_ReturnsSupportable500Error_ForErrorRehearsalRequest()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            options.RehearseException = true;
            var response = await this.state.ApiClient.GetCompanyTransactions(options, 2);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "exception_simulation", "Unexpected error code");
        }*/

        /*
         * Test getting business user attributes for the standard user
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetUserInfo_ReturnsSingleRegion_ForStandardUser()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetUserInfoClaims(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.OK, "Unexpected HTTP status code");
            var claims = JObject.Parse(response.Body);
            var regions = claims.Value<JArray>("regions");
            Assert.True(regions.Count == 1, "Unexpected regions claim");
        }*/

        /*
         * Test getting business user attributes for the admin user
         */
        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetUserClaims_ReturnsAllRegions_ForAdminUser()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseAdminUser();
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetUserInfoClaims(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.OK, "Unexpected HTTP status code");
            System.Console.WriteLine(response.Body);
            var claims = JObject.Parse(response.Body);
            var regions = claims.Value<JArray>("regions");
            Assert.True(regions.Count == 3, "Unexpected regions claim");
        }

        /*
         * Test getting companies
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetCompanies_ReturnsTwoItems_ForStandardUser()
        {
            // Get an access token for the end user of this test
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(jwtOptions);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.OK, "Unexpected HTTP status code");
            var companies = JArray.Parse(response.Body);
            Assert.True(companies.Count == 2, "Unexpected companies list");
        }*/

        /*
         * Test getting companies for the admin user
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetCompanies_ReturnsAllItems_ForAdminUser()
        {
            // Get an access token for the end user of this test
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(this.guestAdminId);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.OK, "Unexpected HTTP status code");
            var companies = JArray.Parse(response.Body);
            Assert.True(companies.Count == 4, "Unexpected companies list");
        }*/

        /*
         * Test getting companies with a malicious JWT access token
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetCompanies_Returns401_ForMaliciousJwt()
        {
            // Get a malicious access token for the end user of this test
            var accessToken = this.state.MockAuthorizationServer.IssueMaliciousAccessToken(this.guestUserId);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanies(options);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "unauthorized", "Unexpected error code");
        }*/

        /*
         * Test getting allowed transactions
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetTransactions_ReturnsAllowedItems_ForCompaniesMatchingTheRegionClaim()
        {
            // Get an access token for the end user of this test
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(this.guestUserId);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanyTransactions(options, 2);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.OK, "Unexpected HTTP status code");
            var payload = JObject.Parse(response.Body);
            var transactions = payload.Value<JArray>("transactions");
            Assert.True(transactions.Count == 8, "Unexpected transactions list");
        }*/

        /*
         * Test getting unauthorized transactions
         */
        /*[Fact]
        [Trait("Category", "Integration")]
        public async Task GetTransactions_ReturnsNotFoundForUser_ForCompaniesNotMatchingTheRegionClaim()
        {
            // Get an access token for the end user of this test
            var accessToken = this.state.MockAuthorizationServer.IssueAccessToken(this.guestUserId);

            // Call the API
            var options = new ApiRequestOptions(accessToken);
            var response = await this.state.ApiClient.GetCompanyTransactions(options, 3);

            // Assert expected results
            Assert.True(response.StatusCode == HttpStatusCode.NotFound, "Unexpected HTTP status code");
            var error = JObject.Parse(response.Body);
            var code = error.Value<string>("code");
            Assert.True(code == "company_not_found", "Unexpected error code");
        }*/
    }
}
