namespace SampleApi.Plumbing.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Middleware;
    using SampleApi.Plumbing.OAuth.ClaimsCaching;
    using SampleApi.Plumbing.Utilities;

    /*
     * The Microsoft specific class for authenticating API requests
     */
    public sealed class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // Constant keys
        private const string StatusCodeKey = "statusCode";
        private const string ClientErrorKey = "clientError";

        public CustomAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory developmentLoggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock)
                : base(options, developmentLoggerFactory, urlEncoder, clock)
        {
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Do not apply authentication to anonymous routes
            var endpoint = this.Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            // Start logging the request, which will be output in the LoggerMiddleware if the authorizer succeeds
            var logEntry = (LogEntry)this.Context.RequestServices.GetService(typeof(ILogEntry));
            logEntry.Start(this.Request);

            try
            {
                // Do the authorization work and get a claims principal
                var authorizer = (OAuthAuthorizer)this.Context.RequestServices.GetService(typeof(OAuthAuthorizer));
                var claimsPrincipal = await authorizer.ExecuteAsync(this.Request);

                // Add identity details to logs
                logEntry.SetIdentity(claimsPrincipal);

                // Set up .NET security
                var ticket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (ClientError clientError)
            {
                // Handle 401 errors
                var handler = new UnhandledExceptionMiddleware();
                handler.HandleException(clientError, this.Context);

                // Finish logging
                logEntry.End(this.Context.Request, this.Context.Response);
                logEntry.Write();

                // Store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd(StatusCodeKey, clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
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
                this.Request.HttpContext.Items.TryAdd(StatusCodeKey, clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
            }
        }

        /*
         * Return authentication error responses to the API caller
         */
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Retrieve items
            var statusCode = this.GetRequestItem<HttpStatusCode>(StatusCodeKey);
            var clientError = this.GetRequestItem<ClientError>(ClientErrorKey);
            if (clientError != null)
            {
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    // Write 401 responses due to invalid tokens
                    await ResponseErrorWriter.WriteInvalidTokenResponse(this.Response, clientError);
                }
                else if (statusCode == HttpStatusCode.InternalServerError)
                {
                    // Write 500 responses due to technical problems during authentication
                    await ResponseErrorWriter.WriteErrorResponse(
                        this.Response,
                        statusCode,
                        clientError.ToResponseFormat());
                }
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