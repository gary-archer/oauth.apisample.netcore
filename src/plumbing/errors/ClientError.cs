namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * An interface that all types of client error object support
     */
    public abstract class ClientError : Exception
    {
        protected ClientError()
        {
        }

        protected ClientError(string message)
            : base(message)
        {
        }

        protected ClientError(string message, Exception inner)
            : base(message, inner)
        {
        }

        // Return the HTTP status code
        public abstract HttpStatusCode StatusCode { get; }

        // Return the error code
        public abstract string ErrorCode { get; }

        // Set additional details returned for API 500 errors
        public abstract void SetExceptionDetails(string area, int instanceId, string utcTime);

        // Return the JSON response format
        public abstract JObject ToResponseFormat();

        // Return the log format
        public abstract JObject ToLogFormat();
    }
}
