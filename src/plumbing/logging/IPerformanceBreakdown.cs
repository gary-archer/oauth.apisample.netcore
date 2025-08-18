namespace FinalApi.Plumbing.Logging
{
    using System;
    using System.Text.Json.Nodes;

    /*
     * Represents a time measurement within an API operation
     */
    public interface IPerformanceBreakdown : IDisposable
    {
        // Set details to associate with the performance breakdown
        void SetDetails(JsonNode value);

        // Create a child breakdown for an inner timing
        IPerformanceBreakdown CreateChild(string name);
    }
}
