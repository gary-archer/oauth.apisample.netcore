namespace SampleApi.Plumbing.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.OAuth;
    using SampleApi.Plumbing.Utilities;

    /*
     * The Microsoft specific class for authenticating API requests
     */
    public sealed class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // Constant keys
        private const string ClientErrorKey = "clientError";

        public CustomAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory developmentLoggerFactory,
            UrlEncoder urlEncoder)
                : base(options, developmentLoggerFactory, urlEncoder)
        {
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Start logging the request, which will be output in the LoggerMiddleware if the authorizer succeeds
            var logEntry = (LogEntry)this.Context.RequestServices.GetService(typeof(ILogEntry));
            logEntry.Start(this.Request);

            try
            {
                // Do the authorization work and get a claims principal
                var oauthFilter = (OAuthFilter)this.Context.RequestServices.GetService(typeof(OAuthFilter));
                var claimsPrincipal = await oauthFilter.ExecuteAsync(this.Request);

                // Add identity details to logs
                logEntry.SetIdentity(claimsPrincipal.JwtClaims.Sub);

                // Set up .NET security so that authorization attributes work in the expected way
                var ticket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception exception)
            {
                // Handle 500 errors due to failed processing
                var handler = new UnhandledExceptionMiddleware();
                var clientError = handler.HandleException(exception, this.Context);

                // Finish logging
                logEntry.End(this.Context.Request, this.Context.Response);
                logEntry.Write();

                // Store results for the below challenge method, which will fire later
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
            }
        }

        /*
         * Return authentication handler error responses to the API client
         */
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var clientError = this.GetRequestItem<ClientError>(ClientErrorKey);
            if (clientError != null)
            {
                await ResponseErrorWriter.WriteErrorResponse(this.Response, clientError);
            }
        }

        /*
         * Get an HTTP request item and manage casting
         */
        private TItem GetRequestItem<TItem>(string name)
        {
            var item = this.Request.HttpContext.Items[name];
            if (item != null)
            {
                return (TItem)item;
            }

            return default(TItem);
        }
    }
}