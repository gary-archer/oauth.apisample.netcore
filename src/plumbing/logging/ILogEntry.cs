namespace FinalApi.Plumbing.Logging
{
    using System.Text.Json.Nodes;

    /*
     * A log entry collects data during an API request and outputs it at the end
     */
    public interface ILogEntry
    {
        // Create a performance breakdown
        IPerformanceBreakdown CreatePerformanceBreakdown(string name);

        // Add arbitrary data
        void AddInfo(JsonNode info);
    }
}
