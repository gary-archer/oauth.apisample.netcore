namespace SampleApi.Plumbing.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Middleware;
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
         * Most requests just return cached results
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var logEntry = (LogEntry)this.Context.RequestServices.GetService(typeof(ILogEntry));
                logEntry.Start(this.Request);

                // Do the authorization work and get claims, including those not in the token
                IAuthorizer authorizer = (IAuthorizer)this.Context.RequestServices.GetService(typeof(IAuthorizer));
                var claims = await authorizer.ExecuteAsync(this.Request);

                // Update the claims holder so that other classes can resolve the claims directly
                var holder = (ClaimsHolder)this.Context.RequestServices.GetService(typeof(ClaimsHolder));
                holder.Value = claims;

                // Add identity details to logs
                logEntry.SetIdentity(claims.Base);

                // Set up .Net security
                var identity = new CustomClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (ClientError clientError)
            {
                // Handle 401 errors
                var handler = new UnhandledExceptionMiddleware();
                handler.HandleException(clientError, this.Context);

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