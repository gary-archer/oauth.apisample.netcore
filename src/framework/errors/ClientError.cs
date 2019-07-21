namespace Framework.Errors
{
    using System;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Client errors returned when there are framework failures
    /// </summary>
    public sealed class ClientError : Exception, IClientError
    {
        // Mandatory fields for both 4xx and 500 errors
        private readonly HttpStatusCode statusCode;
        private readonly string errorCode;

        // Extra fields for 500 errors
        private  string area;
        private int id;
        private string utcTime;

        /// <summary>
        /// Construct from mandatory fields
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <param name="errorCode">An error code for the consumer</param>
        /// <param name="message">A short technical message</param>
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

        /// <summary>
        /// Set extra fields to return to the caller for 500 errors
        /// </summary>
        /// <param name="area">The area where the error occurred</param>
        /// <param name="id">A unique id for the error occurrence</param>
        /// <param name="utcTime">The time when the error occurred</param>
        public void setExceptionDetails(string area, int id, string utcTime) {
            this.area = area;
            this.id = id;
            this.utcTime = utcTime;
        }

        /// <summary>
        /// Return a dynamic object that can be serialized by calling toString
        /// </summary>
        /// <returns>A JSON object representing the response format, without a status code</returns>
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
