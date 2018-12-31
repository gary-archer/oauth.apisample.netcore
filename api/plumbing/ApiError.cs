namespace api.Plumbing
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

        /*
         * Generate a random id when created
         */
        public ApiError(string message) : base(message)
        {
            this.InstanceId = new Random().Next(MIN_ERROR_ID, MAX_ERROR_ID);
        }

        /*
         * Custom fields
         */
        public int StatusCode {get; set;}

        public string Area {get; set;}

        public int InstanceId {get; private set;}

        public string Url {get; set;}
        
        public DateTime Time {get; set;}

        public string Details {get; set;}

        /*
         * Return a dynamic object that can be serialized by calling toString
         */
        public JObject ToLogFormat()
        {
            dynamic data = new JObject();
            data.statusCode = this.StatusCode;
            data.area = this.Area;
            data.message = this.Message;
            data.instanceId = this.InstanceId;
            
            if (data.url != null)
            {
                data.url = this.Url;
            }

            data.time = this.Time;
            data.details = this.Details;
            return data;
        }

        /*
         * Translate to a confidential error that is returned to the API caller, with a reference to the logged details
         */
        public ClientError ToClientError()
        {
            var error = new ClientError(this.StatusCode, this.Area, this.Message);
            error.Id = this.InstanceId;
            return error;
        }
    }
}
