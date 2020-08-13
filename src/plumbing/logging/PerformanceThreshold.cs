namespace SampleApi.Plumbing.Logging
{
    /*
     * A simple performance threshold data object
     */
    internal sealed class PerformanceThreshold
    {
        public string Name { get; set; }

        public int Milliseconds { get; set; }
    }
}