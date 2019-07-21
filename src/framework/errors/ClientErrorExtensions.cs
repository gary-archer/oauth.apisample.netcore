using Newtonsoft.Json.Linq;

namespace Framework.Errors
{
    /// <summary>
    /// Helper extensions for all client errors
    /// </summary>
    public static class ClientErrorExtensions
    {
        /// <summary>
        /// Convert a client error to the log format, including the status code
        /// </summary>
        /// <param name="clientError">The client error</param>
        /// <returns>A status and body</returns>
        public static JObject ToLogFormat(this IClientError clientError)
        {
            dynamic data = new JObject();
            data.statusCode = clientError.StatusCode;
            data.body = clientError.ToResponseFormat();
            return data;
        }
    }
}
