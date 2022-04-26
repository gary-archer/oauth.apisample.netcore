namespace SampleApi.Test.Utils
{
    using System.Net.Http;

    public sealed class ApiRequestOptions
    {
        public ApiRequestOptions(string accessToken)
        {
            this.AccessToken = accessToken;
            this.HttpMethod = HttpMethod.Get;
            this.ApiPath = string.Empty;
        }

        public string AccessToken { get; private set; }

        public HttpMethod HttpMethod { get; set; }

        public string ApiPath { get; set; }
    }
}
