namespace Framework.Errors
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * Our standard representation of an API error, including how it is logged and translated to a client error
     */
    public sealed class ApiError : Exception
    {
        // A range for random error ids
        const int MIN_ERROR_ID = 10000;
        const int MAX_ERROR_ID = 99999;

        // Error fields
        private readonly string errorCode;
        private readonly HttpStatusCode statusCode;
        private readonly string area;
        private readonly int instanceId;
        private readonly string utcTime;
        private string details;

        public ApiError(string errorCode, string userMessage) : base(userMessage)
        {
            this.errorCode = errorCode;
            this.statusCode = HttpStatusCode.InternalServerError;
            this.area = "SampleApi";
            this.instanceId = new Random().Next(MIN_ERROR_ID, MAX_ERROR_ID);
            this.utcTime = DateTime.UtcNow.ToString("s");
            this.details = string.Empty;
        }

        public string Details
        {
            set
            {
                this.details = value;
            }
        }

        /*
         * Return a dynamic object that can be serialized by calling toString
         */
        public JObject ToLogFormat()
        {
            dynamic data = new JObject();
            data.statusCode = this.statusCode;
            data.clientError = this.ToClientError().ToResponseFormat();
            data.serviceError = new JObject();
            data.serviceError.errorCode = this.errorCode;
            data.serviceError.details = this.details;
            return data;
        }

        /*
         * Translate to a confidential error that is returned to the API caller, with a reference to the logged details
         */
        public ClientError ToClientError()
        {
            var error = new ClientError(this.statusCode, this.errorCode, this.Message);
            error.setExceptionDetails(this.area, this.instanceId, this.utcTime);
            return error;
        }
    }
}
