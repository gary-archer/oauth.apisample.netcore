namespace SampleApi.Plumbing.Logging
{
    using System;
    using System.Collections.Generic;
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
        private readonly Func<string, int> performanceThresholdCallback;
        private readonly LogEntryData data;
        private readonly IList<LogEntryData> children;
        private LogEntryData activeChild;
        private bool started;

        /*
         * The default constructor
         */
        public LogEntry(
            string apiName,
            ILog productionLogger)
                : this(apiName, productionLogger, null)
        {
        }

        /*
         * The main constructor
         */
        public LogEntry(
            string apiName,
            ILog productionLogger,
            Func<string, int> performanceThresholdCallback)
        {
            // Store the logger reference
            this.productionLogger = productionLogger;
            this.performanceThresholdCallback = performanceThresholdCallback;

            // Initialise data
            this.data = new LogEntryData();
            this.data.ApiName = apiName;
            this.data.HostName = Environment.MachineName;
            this.children = new List<LogEntryData>();
            this.activeChild = null;

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
                this.data.RequestVerb = request.Method;

                // Include the query in the path if applicable
                this.data.RequestPath = request.Path;
                if (request.QueryString.HasValue)
                {
                    this.data.RequestPath += request.QueryString.Value;
                }

                // Our callers can supply a custom header so that we can keep track of who is calling each API
                var callingApplicationName = request.GetHeader("x-mycompany-api-client");
                if (!string.IsNullOrWhiteSpace(callingApplicationName))
                {
                    this.data.CallingApplicationName = callingApplicationName;
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
        public void SetIdentity(CoreApiClaims claims)
        {
            this.data.ClientId = claims.ClientId;
            this.data.UserId = claims.UserId;
            this.data.UserName = $"{claims.GivenName} {claims.FamilyName}";
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
            var child = this.Current().Performance.CreateChild(name);
            child.Start();
            return child;
        }

        /*
         * Add error details after they have been processed by the exception handler, including denormalised fields
         */
        public void SetServerError(ServerError error)
        {
            this.Current().ErrorData = error.ToLogFormat(this.data.ApiName);
            this.Current().ErrorCode = error.ErrorCode;
            this.Current().ErrorId = error.InstanceId;
        }

        /*
         * Add error details after they have been processed by the exception handler, including denormalised fields
         */
        public void SetClientError(ClientError error)
        {
            this.Current().ErrorData = error.ToLogFormat();
            this.Current().ErrorCode = error.ErrorCode;
        }

        /*
         * Enable arbitrary data to be added
         */
        public void AddInfo(JToken info)
        {
            this.Current().InfoData.Add(info);
        }

        /*
         * Start a child operation, which gets its own JSON log output
         */
        public IDisposable CreateChild(string name)
        {
            // Fail if used incorrectly
            if (this.activeChild != null)
            {
                throw new Exception("The previous child operation must be completed before a new child can be started");
            }

            this.activeChild = new LogEntryData();
            this.activeChild.OperationName = name;
            if (this.performanceThresholdCallback != null)
            {
                this.activeChild.PerformanceThresholdMilliseconds = this.performanceThresholdCallback(name);
            }

            this.activeChild.Performance.Start();
            this.children.Add(this.activeChild);
            return new ChildLogEntry(this);
        }

        /*
        * Complete a child operation
        */
        public void EndChildOperation()
        {
            if (this.activeChild != null)
            {
                this.activeChild.Performance.Dispose();
                this.activeChild = null;
            }
        }

        /*
         * Finish collecting data at the end of the API request
         */
        public void End(HttpRequest request, HttpResponse response)
        {
            // Fill in route details that are not available until now
            this.ProcessRoutes(request.RouteValues);

            // If an active child operation needs ending (due to exceptions) then we do it here
            this.EndChildOperation();

            // Finish performance measurements
            this.data.Performance.Dispose();

            // Record response details
            this.data.StatusCode = response.StatusCode;

            // Finalise this log entry
            this.data.Finalise();

            // Finalise data related to child log entries, to copy data points between parent and children
            foreach (var child in this.children)
            {
                child.Finalise();
                child.UpdateFromParent(this.data);
                this.data.UpdateFromChild(child);
            }
        }

        /*
         * Output any child data and then the parent data
         */
        public void Write()
        {
            foreach (var child in this.children)
            {
                this.WriteDataItem(child);
            }

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

                // Look up the performance threshold for the operation
                if (this.performanceThresholdCallback != null)
                {
                    this.data.PerformanceThresholdMilliseconds = this.performanceThresholdCallback(this.data.OperationName);
                }
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
        * Get the data to use when a child operation needs to be managed
        */
        private LogEntryData Current()
        {
            if (this.activeChild != null)
            {
                return this.activeChild;
            }
            else
            {
                return this.data;
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