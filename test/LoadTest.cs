namespace SampleApi.Test
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    [Category("Load")]
    public class LoadTest
    {
        [Test]
        public void Execute()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.SetError(TestContext.Progress);
            Console.SetOut(TestContext.Progress);
            TestContext.Progress.WriteLine("*** MY LOAD TEST");
            //TestContext.Progress.WriteLine("*** MY LOAD TEST");
            Assert.Pass();
        }
    }
}
