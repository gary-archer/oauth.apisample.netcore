namespace Framework.Errors
{
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An interface that all types of client error object support
    /// </summary>
    public interface IClientError
    {
        // Return the HTTP status code
        HttpStatusCode StatusCode { get; }

        // Return the JSON response format
        JObject ToResponseFormat();
    }
}
