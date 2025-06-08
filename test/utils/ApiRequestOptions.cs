namespace FinalApi.Test.Utils
{
    using System.Net.Http;

    public sealed class ApiRequestOptions
    {
        public ApiRequestOptions(string accessToken)
        {
            this.AccessToken = accessToken;
            this.HttpMethod = HttpMethod.Get;
            this.ApiPath = string.Empty;
            this.RehearseException = false;
        }

        public string AccessToken { get; private set; }

        public HttpMethod HttpMethod { get; set; }

        public string ApiPath { get; set; }

        public bool RehearseException { get; set; }
    }
}
