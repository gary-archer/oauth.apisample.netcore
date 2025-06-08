﻿namespace FinalApi.Plumbing.Logging
{
    using System.Text.Json.Nodes;

    /*
     * Each API request writes a structured log entry containing fields we will query by
     * These operations are exported and this interface can be injected into business logic
     */
    public interface ILogEntry
    {
        // Create a performance breakdown for business logic
        IPerformanceBreakdown CreatePerformanceBreakdown(string name);

        // Add arbitrary data
        void AddInfo(JsonNode info);
    }
}
