namespace Framework.Api.Base.Errors
{
    using System.Net;
    using Newtonsoft.Json.Linq;

    /*
     * An interface that all types of client error object support
     */
    public interface IClientError
    {
        // Return the HTTP status code
        HttpStatusCode StatusCode { get; }

        // Return the error code
        string ErrorCode { get; }

        // Return the JSON response format
        JObject ToResponseFormat();
    }
}
