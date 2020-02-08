namespace Framework.Base.Abstractions
{
    using System;
    using Newtonsoft.Json.Linq;

    /*
     * Represents a time measurement within an API operation
     * These operations are exported and this interface can be used from business logic via the ILogEntry
     */
    public interface IPerformanceBreakdown : IDisposable
    {
        // Set details to associate with the performance breakdown from text
        void SetDetails(string value);

        // Set details to associate with the performance breakdown from an object
        void SetDetails(JToken value);
    }
}
