namespace Framework.Errors
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * The error type for an incorrect client request
     */
    public sealed class ClientError : Exception, IClientError
    {
        // Mandatory fields for both 4xx and 500 errors
        private readonly HttpStatusCode statusCode;
        private readonly string errorCode;

        // Extra fields for 500 errors
        private string area;
        private int id;
        private string utcTime;

        /*
         * All client errors use an error code
         */
        public ClientError(HttpStatusCode statusCode, string errorCode, string message)
            : base(message)
        {
            // Set mandatory fields
            this.statusCode = statusCode;
            this.errorCode = errorCode;

            // Initialise 5xx fields
            this.area = string.Empty;
            this.id = 0;
            this.utcTime = string.Empty;
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        /*
         * A helper method to return a 401 error
         */
        public static ClientError Create401(string reaaon)
        {
            return new ClientError(HttpStatusCode.Unauthorized, "unauthorized", "Missing, invalid or expired access token");
        }

        /*
         * Set extra fields to return to the caller for 500 errors
         */
        public void SetExceptionDetails(string area, int id, string utcTime)
        {
            this.area = area;
            this.id = id;
            this.utcTime = utcTime;
        }

        /*
         * Return a dynamic object that can be serialized by calling toString
         */
        public JObject ToResponseFormat()
        {
            dynamic data = new JObject();
            data.code = this.errorCode;
            data.message = this.Message;

            if (this.id > 0 && this.area.Length > 0 && this.utcTime.Length > 0)
            {
                data.id = this.id;
                data.area = this.area;
                data.utcTime = this.utcTime;
            }

            return data;
        }
    }
}
