namespace SampleApi.Plumbing.Errors
{
    using System;
    using Newtonsoft.Json.Linq;

    /*
     * An interface for errors internal to the API
     */
    public abstract class ApiError : Exception
    {
        public ApiError()
            : base()
        {
        }

        public ApiError(string message)
            : base(message)
        {
        }

        public ApiError(string message, Exception inner)
            : base(message, inner)
        {
        }

        // Return the error code
        public abstract string ErrorCode { get; }

        // Return an instance id used for error lookup
        public abstract int InstanceId { get; }

        // Set details from an object node
        public abstract void SetDetails(JToken details);

        // Return the log format
        public abstract JObject ToLogFormat(string apiName);

        // Return the client error for the API error
        public abstract ClientError ToClientError(string apiName);
    }
}
