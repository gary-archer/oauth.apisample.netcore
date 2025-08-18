namespace FinalApi.Plumbing.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Nodes;

    /*
     * Log data collected during the lifetime of an API request
     */
    internal sealed class LogEntryData
    {
        public LogEntryData()
        {
            this.Id = Guid.NewGuid().ToString();
            this.UtcTime = DateTime.UtcNow;
            this.ApiName = string.Empty;
            this.OperationName = string.Empty;
            this.HostName = string.Empty;
            this.Method = string.Empty;
            this.Path = string.Empty;
            this.ResourceId = string.Empty;
            this.ClientName = string.Empty;
            this.UserId = string.Empty;
            this.StatusCode = 0;
            this.MillisecondsTaken = 0;
            this.PerformanceThresholdMilliseconds = 0;
            this.ErrorCode = string.Empty;
            this.ErrorId = 0;
            this.CorrelationId = string.Empty;
            this.SessionId = string.Empty;
            this.Performance = new PerformanceBreakdown("total");
            this.ErrorData = null;
            this.InfoData = new List<JsonNode>();
            this.Scope = new List<string>();
            this.Claims = null;
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

        // The HTTP method
        public string Method { get; set; }

        // The request path
        public string Path { get; set; }

        // The resource id(s) in the request URL path segments is often useful to query by
        public string ResourceId { get; set; }

        // The calling application name
        public string ClientName { get; set; }

        // The subject claim from the OAuth 2.0 access token
        public string UserId { get; set; }

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
        public JsonNode ErrorData { get; set; }

        // Can be populated in scenarios when extra text is useful
        public List<JsonNode> InfoData { get; private set; }

        // The OAuth scopes from the access token
        public List<string> Scope { get; set; }

        // The OAuth claims from the access token
        public JsonNode Claims { get; set; }

        /*
        * Set fields at the end of a log entry
        */
        public void Finalise()
        {
            this.MillisecondsTaken = this.Performance.MillisecondsTaken;
        }

        /*
         * Output technical support details for troubleshooting but without sensitive data
         */
        public JsonNode ToRequestLog()
        {
            // Output fields used as top level queryable columns
            var output = new JsonObject();
            output["type"] = "request";
            this.OutputString((x) => output["id"] = x, this.Id);
            this.OutputString((x) => output["utcTime"] = x, this.UtcTime.ToString("s"));
            this.OutputString((x) => output["apiName"] = x, this.ApiName);
            this.OutputString((x) => output["operationName"] = x, this.OperationName);
            this.OutputString((x) => output["hostName"] = x, this.HostName);
            this.OutputString((x) => output["method"] = x, this.Method);
            this.OutputString((x) => output["path"] = x, this.Path);
            this.OutputString((x) => output["resourceId"] = x, this.ResourceId);
            this.OutputString((x) => output["clientName"] = x, this.ClientName);
            this.OutputString((x) => output["userId"] = x, this.UserId);
            this.OutputNumber((x) => output["statusCode"] = x, this.StatusCode);
            this.OutputString((x) => output["errorCode"] = x, this.ErrorCode);
            this.OutputNumber((x) => output["errorId"] = x, this.ErrorId);
            this.OutputNumber((x) => output["millisecondsTaken"] = x, this.Performance.MillisecondsTaken, true);
            this.OutputString((x) => output["correlationId"] = x, this.CorrelationId);
            this.OutputString((x) => output["sessionId"] = x, this.SessionId);

            // Output object data, which is looked up via top level fields
            this.OutputPerformance(output);
            this.OutputError(output);
            this.OutputInfo(output);
            return output;
        }

        /*
         * Output audit logs for security visibility but without troubleshooting data
         */
        public JsonNode ToAuditLog()
        {
            var output = new JsonObject();
            output["type"] = "audit";
            this.OutputString((x) => output["id"] = x, this.Id);
            this.OutputString((x) => output["utcTime"] = x, this.UtcTime.ToString("s"));
            this.OutputString((x) => output["apiName"] = x, this.ApiName);
            this.OutputString((x) => output["operationName"] = x, this.OperationName);
            this.OutputString((x) => output["hostName"] = x, this.HostName);
            this.OutputString((x) => output["method"] = x, this.Method);
            this.OutputString((x) => output["path"] = x, this.Path);
            this.OutputString((x) => output["resourceId"] = x, this.ResourceId);
            this.OutputString((x) => output["clientName"] = x, this.ClientName);
            this.OutputString((x) => output["userId"] = x, this.UserId);
            this.OutputNumber((x) => output["statusCode"] = x, this.StatusCode);
            this.OutputString((x) => output["errorCode"] = x, this.ErrorCode);
            this.OutputString((x) => output["correlationId"] = x, this.CorrelationId);
            this.OutputString((x) => output["sessionId"] = x, this.SessionId);

            var isAuthenticated = !string.IsNullOrWhiteSpace(this.UserId);
            output["isAuthenticated"] = isAuthenticated;
            output["isAuthorized"] = isAuthenticated && (this.StatusCode >= 200 && this.StatusCode <= 299);

            if (this.Scope.Count > 0)
            {
                output["scope"] = new JsonArray(this.Scope);
            }

            if (this.Claims != null)
            {
                output["claims"] = this.Claims;
            }

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
        private void OutputPerformance(JsonNode output)
        {
            if (this.Performance.MillisecondsTaken >= this.PerformanceThresholdMilliseconds || this.ErrorId > 0)
            {
                output["performance"] = this.Performance.GetData();
            }
        }

        /*
        * Add error details if applicable
        */
        private void OutputError(JsonNode output)
        {
            if (this.ErrorData != null)
            {
                output["errorData"] = this.ErrorData;
            }
        }

        /*
        * Add info details if applicable
        */
        private void OutputInfo(JsonNode output)
        {
            if (this.InfoData.Count > 0)
            {
                output["infoData"] = new JsonArray(this.InfoData.ToArray());
            }
        }
    }
}
