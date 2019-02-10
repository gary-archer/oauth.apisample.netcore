namespace BasicApi.Plumbing.Errors
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /*
     * Our custom API error data
     */
    public class ApiError : Exception
    {
        // A range for random error ids
        const int MIN_ERROR_ID = 10000;
        const int MAX_ERROR_ID = 99999;

        // Error fields
        private int statusCode;
        private string errorCode;
        private string area;
        private int instanceId;
        private string utcTime;
        private string details;
        private string stack;

        /*
         * Generate a random id when created
         */
        public ApiError(string errorCode, string userMessage) : base(userMessage)
        {
            this.statusCode = 500;
            this.errorCode = errorCode;
            this.area = "BasicApi";
            this.instanceId = new Random().Next(MIN_ERROR_ID, MAX_ERROR_ID);
            this.utcTime = DateTime.UtcNow.ToString("s");
            this.details = String.Empty;
            this.stack = null;
        }

        /*
         * Accessors
         */
        public string Details
        {
            set
            {
                this.details = value;

            }
        }

        public string Stack 
        {
            set
            {
                this.stack = value;

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

            if(!string.IsNullOrWhiteSpace(this.stack))
            {
                data.serviceError.stack = this.stack;
            }
            
            return data;
        }

        /*
         * Translate to a confidential error that is returned to the API caller, with a reference to the logged details
         */
        public ClientError ToClientError()
        {
            var error = new ClientError(this.statusCode, "internal_server_error", this.Message);
            error.setExceptionDetails(this.area, this.instanceId, this.utcTime);
            return error;
        }
    }
}
