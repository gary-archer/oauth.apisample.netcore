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
        public int StatusCode {get; private set;}

        public string Area {get; private set;}

        public int? Id {get; set;}

        public ClientError(int statusCode, string area, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.Area = area;
            this.Id = null;
        }

        /*
         * Return a dynamic object that can be serialized by calling toString
         */
        public JObject ToResponseFormat()
        {
            dynamic data = new JObject();
            data.area = this.Area;
            data.message = this.Message;
            if (this.Id != null)
            {
                data.id = this.Id;
            }

            return data;
        }

        /*
         * Similar to the above but includes the status code
         */
        public JObject ToLogFormat()
        {
            dynamic data = new JObject();
            data.statusCode = this.StatusCode;
            data.body = this.ToResponseFormat();
            return data;
        }
    }
}
