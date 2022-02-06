namespace SampleApi.Plumbing.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using log4net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Utilities;

    /*
     * A basic log entry object with a per request scope
     */
    internal sealed class LogEntry : ILogEntry
    {
        private readonly ILog productionLogger;
        private readonly LogEntryData data;
        private bool started;

        /*
         * The main constructor
         */
        public LogEntry(
            string apiName,
            ILog productionLogger,
            int performanceThresholdMilliseconds = 1000)
        {
            // Store the logger reference
            this.productionLogger = productionLogger;

            // Initialise data
            this.data = new LogEntryData();
            this.data.ApiName = apiName;
            this.data.HostName = Environment.MachineName;
            this.data.PerformanceThresholdMilliseconds = performanceThresholdMilliseconds;

            // Set a flag to prevent re-entrancy
            this.started = false;
        }

        /*
         * Start collecting data before calling the API's business logic
         */
        public void Start(HttpRequest request)
        {
            // Start is called by both the authorizer and the logger middleware so only add start data once
            if (!this.started)
            {
                this.started = true;

                // Read request details
                this.data.Performance.Start();
                this.data.Method = request.Method;

                // Include the query in the path if applicable
                this.data.Path = request.Path;
                if (request.QueryString.HasValue)
                {
                    this.data.Path += request.QueryString.Value;
                }

                // Our callers can supply a custom header so that we can keep track of who is calling each API
                var clientApplicationName = request.GetHeader("x-mycompany-api-client");
                if (!string.IsNullOrWhiteSpace(clientApplicationName))
                {
                    this.data.ClientApplicationName = clientApplicationName;
                }

                // Use the correlation id from request headers or create one
                var correlationId = request.GetHeader("x-mycompany-correlation-id");
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    this.data.CorrelationId = correlationId;
                }
                else
                {
                    this.data.CorrelationId = Guid.NewGuid().ToString();
                }

                // Log an optional session id if supplied
                var sessionId = request.GetHeader("x-mycompany-session-id");
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    this.data.SessionId = sessionId;
                }
            }
        }

        /*
         * Add identity details for secured requests
         */
        public void SetIdentity(ClaimsPrincipal claims)
        {
            this.data.UserId = claims.GetSubject();
        }

        /*
         * An internal method for setting the operation name
         */
        public void SetOperationName(string name)
        {
            this.data.OperationName = name;
        }

        /*
         * Create a child performance breakdown when requested
         */
        public IPerformanceBreakdown CreatePerformanceBreakdown(string name)
        {
            return this.data.Performance.CreateChild(name);
        }

        /*
         * Add error details after they have been processed by the exception handler, including denormalised fields
         */
        public void SetServerError(ServerError error)
        {
            this.data.ErrorData = error.ToLogFormat(this.data.ApiName);
            this.data.ErrorCode = error.ErrorCode;
            this.data.ErrorId = error.InstanceId;
        }

        /*
         * Add error details after they have been processed by the exception handler, including denormalised fields
         */
        public void SetClientError(ClientError error)
        {
            this.data.ErrorData = error.ToLogFormat();
            this.data.ErrorCode = error.ErrorCode;
        }

        /*
         * Enable arbitrary data to be added
         */
        public void AddInfo(JToken info)
        {
            this.data.InfoData.Add(info);
        }

        /*
         * Finish collecting data at the end of the API request
         */
        public void End(HttpRequest request, HttpResponse response)
        {
            // Fill in route details that are not available until now
            this.ProcessRoutes(request.RouteValues);

            // Finish performance measurements
            this.data.Performance.Dispose();

            // Record response details
            this.data.StatusCode = response.StatusCode;

            // Finalise this log entry
            this.data.Finalise();
        }

        /*
         * Output this log entry
         */
        public void Write()
        {
            this.WriteDataItem(this.data);
        }

        /*
         * Derive the resource id and operation name from route details
         */
        private void ProcessRoutes(RouteValueDictionary routes)
        {
            // Set the name of the operation being called
            var operationName = routes["action"];
            if (operationName != null)
            {
                this.data.OperationName = operationName.ToString();
            }

            // Capture template ids in URL path segments
            var ids = new List<string>();
            foreach (var route in routes)
            {
                if (route.Key != "action" && route.Key != "controller")
                {
                    var id = route.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        ids.Add(id);
                    }
                }
            }

            // Set them as the resource id
            if (ids.Count > 0)
            {
                this.data.ResourceId = string.Join('/', ids);
            }
        }

        /*
         * Write a single data item
         */
        private void WriteDataItem(LogEntryData item)
        {
            // Get the object to log
            var logData = item.ToLogFormat();

            // Output it
            if (item.ErrorData != null)
            {
                this.productionLogger.Error(logData);
            }
            else
            {
                this.productionLogger.Info(logData);
            }
        }
    }
}