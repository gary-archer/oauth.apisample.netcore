namespace SampleApi.Test.TokenIssuer
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SampleApi.Test.Utils;

    /*
     * Test the API in isolation, without any dependencies on the Authorization Server
     */
    [TestFixture]
    public class IntegrationTests
    {
        // The real subject claim values for my two online test users
        private string guestUserId  = "a6b404b1-98af-41a2-8e7f-e4061dc0bf86";
        private string guestAdminId = "77a97e5b-b748-45e5-bb6f-658e85b2df91";

        // A class to issue our own JWTs for testing
        private TokenIssuer tokenIssuer;

        /*
         * Initialize mock token issuing and wiremock
         */
        [OneTimeSetUp]
        public void Setup()
        {
            this.tokenIssuer = new TokenIssuer();
            var keyset = this.tokenIssuer.GetTokenSigningPublicKeys();
            TestContext.Progress.WriteLine(keyset);
        }
        
        /*
         * Clean up resources after all tests have completed
         */
        [OneTimeTearDown]
        public void Teardown()
        {
            this.tokenIssuer.Dispose();
        }

        [Test]
        public void GetUserClaims_ReturnsSingleRegion_ForStandardUser()
        {
            var accessToken = tokenIssuer.IssueAccessToken(guestUserId);
            TestContext.Progress.WriteLine(accessToken);
            Assert.Pass();
        }
    }
}