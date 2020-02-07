namespace Framework.Api.Base.Logging
{
    using Framework.Api.Base.Errors;
    using Framework.Api.OAuth.Claims;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;

    /*
     * A basic log entry object with a per request scope
     */
    public class LogEntry
    {
        private string requestPath;
        private string userId;
        private int statusCode;
        private IClientError clientError;
        private ApiError apiError;

        public LogEntry()
        {
            this.clientError = null;
            this.apiError = null;
            this.requestPath = string.Empty;
            this.userId = string.Empty;
            this.statusCode = 0;
        }

        /*
         * Start request logging
         */
        public void Start(HttpRequest request)
        {
            this.requestPath = request.Path.ToString();
        }

        /*
         * Record identity details
         */
        public void SetIdentity(CoreApiClaims claims)
        {
            this.userId = claims.UserId;
        }

        /*
         * End request logging
         */
        public void End(HttpResponse response)
        {
            if (response != null)
            {
                this.statusCode = response.StatusCode;
            }

            this.Write();
        }

        /*
         * Add client error details
         */
        public void AddClientError(IClientError error)
        {
            this.clientError = error;
        }

        /*
         * Add API error details
         */
        public void AddApiError(ApiError error)
        {
            this.apiError = error;
        }

        /*
         * Output the data
         */
        private void Write()
        {
            // Produce a dynamic JSON object
            dynamic data = new JObject();

            if (this.statusCode != 0)
            {
                data.statusCode = this.statusCode;
            }

            if (!string.IsNullOrWhiteSpace(this.requestPath))
            {
                data.requestPath = this.requestPath;
            }

            if (!string.IsNullOrWhiteSpace(this.userId))
            {
                data.userId = this.userId;
            }

            if (this.clientError != null)
            {
                data.clientError = this.clientError.ToResponseFormat();
            }

            if (this.apiError != null)
            {
                data.clientError = this.apiError.ToClientError().ToResponseFormat();
                data.serviceError = new JObject();
                data.serviceError.errorCode = this.apiError.ToLogFormat();
            }

            // Ask the logger to write it
            var factory = new LoggerFactory();
            var logger = factory.GetProductionLogger();
            logger.Info(data);
        }
    }
}