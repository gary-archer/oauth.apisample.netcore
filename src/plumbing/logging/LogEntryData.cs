namespace SampleApi.Plumbing.Logging
{
    using System;
    using Newtonsoft.Json.Linq;

    /*
     * Each API request writes a structured log entry containing fields we will query by
     * It also writes JSON blobs whose fields are not designed to be queried
     */
    internal sealed class LogEntryData
    {
        public LogEntryData()
        {
            // Queryable fields
            this.Id = Guid.NewGuid().ToString();
            this.UtcTime = DateTime.UtcNow;
            this.ApiName = string.Empty;
            this.OperationName = string.Empty;
            this.HostName = string.Empty;
            this.RequestVerb = string.Empty;
            this.ResourceId = string.Empty;
            this.RequestPath = string.Empty;
            this.ClientApplicationName = string.Empty;
            this.UserOAuthId = string.Empty;
            this.StatusCode = 0;
            this.MillisecondsTaken = 0;
            this.PerformanceThresholdMilliseconds = 0;
            this.ErrorCode = string.Empty;
            this.ErrorId = 0;
            this.CorrelationId = string.Empty;
            this.SessionId = string.Empty;

            // Objects that are not directly queryable
            this.Performance = new PerformanceBreakdown("total");
            this.ErrorData = null;
            this.InfoData = new JArray();
        }

        // A unique generated client side id, which becomes the unique id in the aggregated logs database
        public string Id { get; set; }

        // The time when the API received the request
        public DateTime UtcTime { get; set; }

        // The name of the API
        public string ApiName { get; set; }

        // The operation called
        public string OperationName { get; set; }

        // The host on which the request was processed
        public string HostName { get; set; }

        // The HTTP verb
        public string RequestVerb { get; set; }

        // The resource id(s) in the request URL path segments is often useful to query by
        public string ResourceId { get; set; }

        // The request path
        public string RequestPath { get; set; }

        // The calling application name
        public string ClientApplicationName { get; set; }

        // The subject claim from the OAuth 2.0 access token
        public string UserOAuthId { get; set; }

        // The status code returned
        public int StatusCode { get; set; }

        // The time taken in API code
        public int MillisecondsTaken { get; set; }

        // A time beyond which performance is considered 'slow'
        public int PerformanceThresholdMilliseconds { get; set; }

        // The error code for requests that failed
        public string ErrorCode { get; set; }

        // The specific error instance id, for 500 errors
        public int ErrorId { get; set; }

        // The correlation id, used to link related API requests together
        public string CorrelationId { get; set; }

        // A session id, to group related calls from a client together
        public string SessionId { get; set; }

        // An object containing performance data, written when performance is slow
        public PerformanceBreakdown Performance { get; private set;  }

        // An object containing error data, written for failed requests
        public JObject ErrorData { get; set; }

        // Can be populated in scenarios when extra text is useful
        public JArray InfoData { get; private set; }

        /*
        * Set fields at the end of a log entry
        */
        public void Finalise()
        {
            this.MillisecondsTaken = this.Performance.MillisecondsTaken;
        }

        /*
        * Produce the output format
        */
        public JObject ToLogFormat()
        {
            // Output fields used as top level queryable columns
            dynamic output = new JObject();
            this.OutputString((x) => output.id = x, this.Id);
            this.OutputString((x) => output.utcTime = x, this.UtcTime.ToString("s"));
            this.OutputString((x) => output.apiName = x, this.ApiName);
            this.OutputString((x) => output.operationName = x, this.OperationName);
            this.OutputString((x) => output.hostName = x, this.HostName);
            this.OutputString((x) => output.requestVerb = x, this.RequestVerb);
            this.OutputString((x) => output.resourceId = x, this.ResourceId);
            this.OutputString((x) => output.requestPath = x, this.RequestPath);
            this.OutputString((x) => output.clientApplicationName = x, this.ClientApplicationName);
            this.OutputString((x) => output.userOAuthId = x, this.UserOAuthId);
            this.OutputNumber((x) => output.statusCode = x, this.StatusCode);
            this.OutputString((x) => output.errorCode = x, this.ErrorCode);
            this.OutputNumber((x) => output.errorId = x, this.ErrorId);
            this.OutputNumber((x) => output.millisecondsTaken = x, this.Performance.MillisecondsTaken, true);
            this.OutputNumber((x) => output.millisecondsThreshold = x, this.PerformanceThresholdMilliseconds, true);
            this.OutputString((x) => output.correlationId = x, this.CorrelationId);
            this.OutputString((x) => output.sessionId = x, this.SessionId);

            // Output object data, which is looked up via top level fields
            this.OutputPerformance(output);
            this.OutputError(output);
            this.OutputInfo(output);
            return output;
        }

        /*
        * Indicate whether an error entry
        */
        public bool IsError()
        {
            return this.ErrorData != null;
        }

        /*
        * Add a string to the output unless empty
        */
        private void OutputString(Action<string> setter, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                setter(value);
            }
        }

        /*
        * Add a number to the output unless zero or forced
        */
        private void OutputNumber(Action<int> setter, int value, bool force = false)
        {
            if (value > 0 || force)
            {
                setter(value);
            }
        }

        /*
        * Add the performance breakdown if the threshold has been exceeded or there has been a 500 error
        */
        private void OutputPerformance(dynamic output)
        {
            if (this.Performance.MillisecondsTaken >= this.PerformanceThresholdMilliseconds || this.ErrorId > 0)
            {
                output.performance = this.Performance.GetData();
            }
        }

        /*
        * Add error details if applicable
        */
        private void OutputError(dynamic output)
        {
            if (this.ErrorData != null)
            {
                output.errorData = this.ErrorData;
            }
        }

        /*
        * Add info details if applicable
        */
        private void OutputInfo(dynamic data)
        {
            if (this.InfoData.Count > 0)
            {
                data.infoData = this.InfoData;
            }
        }
    }
}
