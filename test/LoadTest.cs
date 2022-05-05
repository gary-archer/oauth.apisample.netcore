namespace Test2
{
    using System;
    using Xunit;

    /*
     * A basic load test to ensure that the API behaves correctly when there are concurrent requests
     */
    public class LoadTest : IDisposable
    {
        private readonly string sessionId;
        private readonly int totalCount;
        private readonly int errorCount;

        /*
         * Setup that runs at the start of the test run
         */
        public LoadTest()
        {
            this.sessionId = Guid.NewGuid().ToString();
            this.totalCount = 0;
            this.errorCount = 0;
        }

        /*
         * Teardown that runs when the load test has completed
         */
        public void Dispose()
        {
        }

        /*
         * Run the load test
         */
        [Fact]
        [Trait("Category", "Load")]
        public void Run()
        {
            // Show a startup message
            Console.WriteLine();
            var startTime = DateTime.UtcNow;
            this.OutputMessage(ConsoleColor.Blue, $"Load test session {this.sessionId} starting at {startTime.ToString("s")}");
            Console.WriteLine();

            var headings = new string[]
            {
                "OPERATION".PadRight(38, ' '),
                "CORRELATION-ID".PadRight(38, ' '),
                "START-TIME".PadRight(30, ' '),
                "MILLISECONDS-TAKEN".PadRight(21, ' '),
                "STATUS-CODE".PadRight(14, ' '),
                "ERROR-CODE".PadRight(24, ' '),
                "ERROR-ID".PadRight(12, ' '),
            };
            var header = string.Join(string.Empty, headings);
            this.OutputMessage(ConsoleColor.Yellow, header);
            Console.WriteLine();

            // Show a summary end message to finish
            var endTime = DateTime.UtcNow;
            var timeTaken = (endTime - startTime).TotalMilliseconds;
            this.OutputMessage(
                ConsoleColor.Blue,
                $"Load test session {this.sessionId} completed in {timeTaken} milliseconds: {this.errorCount} errors from {this.totalCount} requests");
            Console.WriteLine();
        }

        /*
         * Do some initial work to get multiple access tokens
         */
        private void GetAccessTokens()
        {
        }

        /*
         * Run the main body of API requests, including some invalid requests that trigger errors
         */
        private void SendLoadTestRequests()
        {
        }

        /*
         * A utility to output in a desired colour
         */
        private void OutputMessage(ConsoleColor color, string message)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(message);
        }
    }
}
