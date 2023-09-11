namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * A default representation of a server error, including how it is logged and translated to a client error
     */
    internal sealed class ServerErrorImpl : ServerError
    {
        // A range for random error ids
        private const int MinErrorId = 10000;
        private const int MaxErrorId = 99999;

        // Fields to log after error translation
        private readonly string errorCode;
        private readonly HttpStatusCode statusCode;
        private readonly int instanceId;
        private readonly string utcTime;
        private JToken details;

        /*
         * The default constructor
         */
        public ServerErrorImpl(string errorCode, string userMessage)
            : this(errorCode, userMessage, null)
        {
        }

        /*
         * The main constructor
         */
        public ServerErrorImpl(string errorCode, string userMessage, Exception inner)
            : base(userMessage, inner)
        {
            this.errorCode = errorCode;
            this.statusCode = HttpStatusCode.InternalServerError;
            this.instanceId = new Random().Next(MinErrorId, MaxErrorId);
            this.utcTime = DateTime.UtcNow.ToString("s");
            this.details = null;
        }

        public override string ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        public override int InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public override void SetDetails(JToken details)
        {
            this.details = details;
        }

        /*
         * Return a dynamic object that can be serialized by calling toString
         */
        public override JObject ToLogFormat(string apiName)
        {
            dynamic data = new JObject();
            data.statusCode = this.statusCode;
            data.clientError = this.ToClientError(apiName).ToResponseFormat();
            data.serviceError = new JObject();
            data.serviceError.errorCode = this.errorCode;
            data.serviceError.details = this.GetErrorDetails();

            // Output the stack trace of the original error
            var stack = this.GetOriginalException().StackTrace;
            if (stack != null)
            {
                var frames = stack.Split('\n');
                if (frames.Length > 0)
                {
                    data.serviceError.stack = new JArray();
                    foreach (var frame in frames)
                    {
                        data.serviceError.stack.Add(frame.Trim());
                    }
                }
            }

            return data;
        }

        /*
         * Translate to a confidential error that is returned to the API caller, with a reference to the logged details
         */
        public override ClientError ToClientError(string apiName)
        {
            var error = ErrorFactory.CreateClientError(this.statusCode, this.errorCode, this.Message);
            error.SetExceptionDetails(apiName, this.instanceId, this.utcTime);
            return error;
        }

        /*
         * Get an exception's original cause for call stack reporting
         */
        private Exception GetOriginalException()
        {
            Exception ex = this;
            Exception inner = this;
            while (inner != null)
            {
                inner = inner.InnerException;
                if (inner != null)
                {
                    ex = inner;
                }
            }

            return ex;
        }

        /*
         * Get all error details
         */
        private string GetErrorDetails()
        {
            var parts = new List<string>();

            // Add any details passed in
            if (this.details != null)
            {
                parts.Add(this.details.ToString());
            }

            // Add any details from received exceptions
            var inner = this.InnerException;
            while (inner != null)
            {
                parts.Add(inner.Message);
                inner = inner.InnerException;
            }

            // Combine them all as text
            return string.Join(' ', parts);
        }
    }
}
