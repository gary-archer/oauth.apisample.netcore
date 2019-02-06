namespace BasicApi.Plumbing.Errors
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /*
     * The client error information returned over the wire
     */
    public class ClientError : Exception
    {
        /*
         * Mandatory fields for both 4xx and 500 errors
         */
        private int statusCode;
        private string errorCode;


        /*
         * Extra fields for 500 errors
         */
         private  string area;
         private int id;
         private string utcTime;

        /*
         * Construct from mandatory fields
         */
        public ClientError(int statusCode, string errorCode, string message)
            : base(message)
        {
            // Set mandatory fields
            this.statusCode = statusCode;
            this.errorCode = errorCode;

            // Initialise 5xx fields
            this.area = String.Empty;
            this.id = 0;
            this.utcTime = String.Empty;
        }

        /*
         * Accessors
         */
        public int StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        /*
         * Set extra fields to return to the caller for 500 errors
         */
        public void setExceptionDetails(string area, int id, string utcTime) {
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

        /*
         * Similar to the above but includes the status code
         */
        public JObject ToLogFormat()
        {
            dynamic data = new JObject();
            data.statusCode = this.statusCode;
            data.body = this.ToResponseFormat();
            return data;
        }
    }
}
