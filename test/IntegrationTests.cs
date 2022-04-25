namespace SampleApi.Test.TokenIssuer
{
    using NUnit.Framework;
    using SampleApi.Test.Utils;

    [TestFixture]
    public class IntegrationTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var issuer = new TokenIssuer();
            var accessToken = issuer.IssueAccessToken("gary");
            TestContext.Progress.WriteLine(accessToken);
        }

        [Test]
        public void Test1()
        {
            TestContext.Progress.WriteLine("Test 1");
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            TestContext.Progress.WriteLine("Test 2");
            Assert.Pass();
        }
    }
}