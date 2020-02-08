namespace Framework.Base.Abstractions
{
    using System;

    /*
     * Represents a time measurement within an API operation
     * These operations are exported and this interface can be used from business logic via the ILogEntry
     */
    public interface IPerformanceBreakdown : IDisposable
    {
        // Set details to associate with the performance breakdown
        // One use case would be to log SQL with input parameters
        void SetDetails(string value);
    }
}
