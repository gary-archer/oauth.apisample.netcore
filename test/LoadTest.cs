namespace Test2
{
    using System;
    using Xunit;

    public class LoadTest : IDisposable
    {
        public LoadTest()
        {
            System.Console.WriteLine("SETUP");
        }

        public void Dispose()
        {
            System.Console.WriteLine("TEARDOWN");
        }

        [Fact]
        [Trait("Category", "Load")]
        public void Run()
        {
            System.Console.ForegroundColor = System.ConsoleColor.Blue;
            System.Console.WriteLine("DEMO TEST");

            System.Console.ForegroundColor = System.ConsoleColor.Yellow;
            System.Console.WriteLine("DEMO TEST END");
        }
    }
}
