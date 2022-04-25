namespace SampleApi.Test.TokenIssuer
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SampleApi.Test.Utils;

    [TestFixture]
    public class IntegrationTests
    {
        private string guestUserId  = "a6b404b1-98af-41a2-8e7f-e4061dc0bf86";
        private string guestAdminId = "77a97e5b-b748-45e5-bb6f-658e85b2df91";


        [OneTimeSetUp]
        public async Task Setup()
        {
            var tokenIssuer = new TokenIssuer();
            var keyset = tokenIssuer.GetTokenSigningPublicKeys();
            TestContext.Progress.WriteLine(keyset);

            var accessToken = tokenIssuer.IssueAccessToken(guestUserId);
            TestContext.Progress.WriteLine(accessToken);
        }

        [Test]
        public void GetUserClaims_ReturnsSingleRegion_ForStandardUser()
        {
            TestContext.Progress.WriteLine("Test 1");
            Assert.Pass();
        }
    }
}