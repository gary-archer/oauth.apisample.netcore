namespace SampleApi.Test.Utils
{
    using System.Net;

    public sealed class ApiResponse
    {
        public ApiResponse(HttpStatusCode statusCode, string body, ApiResponseMetrics metrics)
        {
            this.StatusCode = statusCode;
            this.Body = body;
            this.Metrics = metrics;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public string Body { get; set; }

        public ApiResponseMetrics Metrics { get; private set; }
    }
}
