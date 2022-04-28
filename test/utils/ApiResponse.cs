namespace SampleApi.Test.Utils
{
    using System.Net;

    public sealed class ApiResponse
    {
        public ApiResponse(HttpStatusCode statusCode, string body)
        {
            this.StatusCode = statusCode;
            this.Body = body;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public string Body { get; set; }
    }
}
