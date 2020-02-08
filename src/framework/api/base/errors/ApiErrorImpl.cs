namespace Framework.Api.Base.Errors
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * Our standard representation of an API error, including how it is logged and translated to a client error
     */
    internal sealed class ApiErrorImpl : ApiError
    {
        // A range for random error ids
        private const int MinErrorId = 10000;
        private const int MaxErrorId = 99999;

        // Fields to log after error translation
        private readonly string errorCode;
        private readonly HttpStatusCode statusCode;
        private readonly int instanceId;
        private readonly string utcTime;
        private JObject details;

        /*
         * The default constructor
         */
        public ApiErrorImpl(string errorCode, string userMessage)
            : this(errorCode, userMessage, null)
        {
        }

        /*
         * The main constructor
         */
        public ApiErrorImpl(string errorCode, string userMessage, Exception inner)
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

        public override void SetDetails(string details)
        {
            dynamic data = new JObject();
            data.details = details;
            this.details = data;
        }

        public override void SetDetails(JObject details)
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
            data.serviceError.details = this.details;
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
    }
}
