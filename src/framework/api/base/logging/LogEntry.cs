namespace Framework.Api.Base.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Api.Base.Claims;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Utilities;
    using log4net;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;

    /*
     * A basic log entry object with a per request scope
     */
    public class LogEntry
    {
        // Data logged
        private LogEntryData data;
        private IList<LogEntryData> children;
        private LogEntryData activeChild;

        // A flag to avoid repeated logging
        private bool started;

        // Performance details
        private int defaultThresholdMilliseconds;
        private IEnumerable<PerformanceThreshold> performanceThresholdOverrides;

        /*
         * A log entry is created once per API request
         */
        public LogEntry(string apiName)
        {
            this.data = new LogEntryData();
            this.data.ApiName = apiName;
            this.data.HostName = Environment.MachineName;
            this.children = new List<LogEntryData>();
            this.activeChild = null;
            this.started = false;
        }

        /*
         * Set default performance details after creation
         */
        public void SetPerformanceThresholds(int defaultMilliseconds, IEnumerable<PerformanceThreshold> overrides)
        {
            this.defaultThresholdMilliseconds = defaultMilliseconds;
            this.data.PerformanceThresholdMilliseconds = this.defaultThresholdMilliseconds;
            this.performanceThresholdOverrides = overrides;
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
                this.data.RequestPath = request.Path;

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

                /*
                // Calculate the operation name from request.route, which is not available at the start of the request
                if (this._routeMetadataHandler) {
                    const metadata = this._routeMetadataHandler.getOperationRouteInfo(request);
                    if (metadata) {

                        // Record the operation name and also ensure that the correct performance threshold is used
                        this.data.operationName = metadata.operationName;
                        this.data.performanceThresholdMilliseconds = this._getPerformanceThreshold(this.data.operationName);

                        // Also log URL path segments for resource ids
                        this.data.resourceId = metadata.resourceIds.join('/');
                    }
                }
                */
            }
        }

        /*
        * Add identity details for secured requests
        */
        public void SetIdentity(CoreApiClaims claims)
        {
            this.data.ClientId = claims.ClientId;
            this.data.UserId = claims.UserId;
            this.data.UserName = $"{claims.GivenName} ${claims.FamilyName}";
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
        public PerformanceBreakdown CreatePerformanceBreakdown(string name)
        {
            var child = this.Current().Performance.CreateChild(name);
            child.Start();
            return child;
        }

        /*
        * Add error details after they have been processed by the exception handler, including denormalised fields
        */
        public void SetApiError(ApiError error)
        {
            this.Current().ErrorData = error.ToLogFormat(this.data.ApiName);
            this.Current().ErrorCode = error.ErrorCode;
            this.Current().ErrorId = error.InstanceId;
        }

        /*
        * Add error details after they have been processed by the exception handler, including denormalised fields
        */
        public void SetClientError(IClientError error)
        {
            this.Current().ErrorData = error.ToLogFormat();
            this.Current().ErrorCode = error.ErrorCode;
        }

        /*
        * Enable free text to be added to production logs, though this should be avoided in most cases
        */
        public void AddInfo(JObject info)
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

            // Initialise the child
            this.activeChild = new LogEntryData();
            this.activeChild.PerformanceThresholdMilliseconds = this.GetPerformanceThreshold(name);
            this.activeChild.OperationName = name;
            this.activeChild.Performance.Start();

            // Add to the parent and return an object to simplify disposal
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
        public void End(HttpResponse response, ILog logger)
        {
            if (response != null)
            {
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

            // Finally output the data
            this.Write(logger);
        }

        /*
        * Output any child data and then the parent data
        */
        private void Write(ILog logger)
        {
            foreach (var child in this.children)
            {
                this.WriteDataItem(child, logger);
            }

            this.WriteDataItem(this.data, logger);
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
        * Given an operation name, find its performance threshold
        */
        private int GetPerformanceThreshold(string name)
        {
            var found = this.performanceThresholdOverrides.FirstOrDefault(
                o => o.Name.ToLowerInvariant() == name.ToLowerInvariant());
            if (found != null)
            {
                return found.Milliseconds;
            }

            return this.defaultThresholdMilliseconds;
        }

        /*
        * Write a single data item
        */
        private void WriteDataItem(LogEntryData item, ILog logger)
        {
            // Get the object to log
            var logData = item.ToLogFormat();

            // Output it
            if (item.ErrorData != null)
            {
                logger.Error(logData);
            }
            else
            {
                logger.Info(logData);
            }
        }
    }
}