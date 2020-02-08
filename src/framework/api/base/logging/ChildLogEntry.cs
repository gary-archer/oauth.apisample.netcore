namespace Framework.Api.Base.Logging
{
    using System;

    /*
     * A helper to support the dispose pattern for child operations
     */
    public class ChildLogEntry : IDisposable
    {
        private readonly LogEntry logEntry;

        public ChildLogEntry(LogEntry logEntry)
        {
            this.logEntry = logEntry;
        }

        public void Dispose()
        {
            this.logEntry.EndChildOperation();
        }
    }
}