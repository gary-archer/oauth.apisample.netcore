namespace SampleApi.Test.Utils
{
    using System;

    /*
     * Some metrics once an API call completes
     */
    public sealed class ApiResponseMetrics
    {
        public ApiResponseMetrics(string operation)
        {
            this.Operation = operation;
            this.StartTime = DateTime.MinValue;
            this.CorrelationId = string.Empty;
            this.MillisecondsTaken = 0;
        }

        public string Operation { get; private set; }

        public DateTime StartTime { get; set; }

        public string CorrelationId { get; set; }

        public int MillisecondsTaken { get; set; }
    }
}
