namespace Framework.Api.Base.Errors
{
    using Newtonsoft.Json.Linq;

    /*
     * Helper extensions for all client errors
     */
    public static class ClientErrorExtensions
    {
        /*
         * Convert a client error to the log format, including the status code
         */
        public static JObject ToLogFormat(this IClientError clientError)
        {
            dynamic data = new JObject();
            data.statusCode = clientError.StatusCode;
            data.body = clientError.ToResponseFormat();
            return data;
        }
    }
}
