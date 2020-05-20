namespace SampleApi.Host.Plumbing.Logging
{
    using System;
    using Newtonsoft.Json.Linq;

    /*
     * Represents a time measurement within an API operation
     * These operations are exported and this interface can be used from business logic via the ILogEntry
     */
    public interface IPerformanceBreakdown : IDisposable
    {
        // Set details to associate with the performance breakdown from an object
        void SetDetails(JToken value);
    }
}
