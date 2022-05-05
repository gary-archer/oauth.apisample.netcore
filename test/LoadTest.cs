namespace SampleApi.LoadTest
{
    using NUnit.Framework;

    /*
     * Run a basic load test
     */
    [TestFixture]
    [Category("Load")]
    public class LoadTest
    {
        /*
         * Initialize mock token issuing and wiremock
         */
        [OneTimeSetUp]
        public void Setup()
        {
        }

        /*
         * Clean up resources after all tests have completed
         */
        [OneTimeTearDown]
        public void Teardown()
        {
        }

        /*
         * Run the load test
         */
        [Test]
        public void Run()
        {
            System.Console.ForegroundColor = System.ConsoleColor.Blue;
            TestContext.Progress.WriteLine("TEST OUTPUT");
            Assert.Pass();
        }
    }
}
