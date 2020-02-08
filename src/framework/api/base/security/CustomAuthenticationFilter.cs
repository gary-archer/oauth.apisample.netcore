namespace Framework.Api.Base.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Framework.Api.Base.Configuration;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Middleware;
    using Framework.Api.Base.Utilities;
    using Framework.Base.Abstractions;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;

    /*
     * The Microsoft specific class which wraps the authorizer we pass in
     */
    public sealed class CustomAuthenticationFilter<T> : AuthenticationHandler<T>
        where T : AuthenticationSchemeOptions, new()
    {
        // Constants used as keys
        private const string StatusCodeKey = "statusCode";
        private const string ClientErrorKey = "clientError";

        // Framework objects
        private readonly IAuthorizer authorizer;
        private readonly LogEntry logEntry;
        private readonly FrameworkConfiguration configuration;
        private readonly ApplicationExceptionHandler applicationHandler;

        /*
         * The first 4 parmameters are required by Microsoft and of no interest to this sample
         */
        public CustomAuthenticationFilter(
            IOptionsMonitor<T> options,
            Microsoft.Extensions.Logging.ILoggerFactory developmentLoggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock,
            IAuthorizer authorizer,
            FrameworkConfiguration configuration,
            ILogEntry logEntry,
            ApplicationExceptionHandler applicationHandler)
                : base(options, developmentLoggerFactory, urlEncoder, clock)
        {
            this.authorizer = authorizer;
            this.configuration = configuration;
            this.logEntry = (LogEntry)logEntry;
            this.applicationHandler = applicationHandler;
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Start logging of secure requests here
                this.logEntry.Start(this.Request);

                // Do the authentication work
                var claims = await this.authorizer.Execute(this.Request);

                // Add identity details to logs
                this.logEntry.SetIdentity(claims);

                // Get claims into a collection
                var claimsList = new List<Claim>();
                claims.Output(claimsList);

                // Set up the .Net security context
                var identity = new ClaimsIdentity(claimsList, this.Scheme.Name, JwtClaimTypes.Subject, string.Empty);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (ClientError clientError)
            {
                // If there is an error then handle it via the exception handler
                var handler = new UnhandledExceptionMiddleware();
                handler.HandleError(clientError, this.configuration, this.logEntry, this.applicationHandler);

                // Store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd(StatusCodeKey, clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
            }
            catch (Exception exception)
            {
                // If there is an error then handle it via the exception handler
                var handler = new UnhandledExceptionMiddleware();
                var clientError = handler.HandleError(exception, this.configuration, this.logEntry, this.applicationHandler);

                // Store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd(StatusCodeKey, clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
            }
        }

        /*
         * This returns any authentication error responses to the API caller
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
                    await ResponseErrorWriter.WriteInvalidTokenResponse(this.Request, this.Response, clientError);
                }
                else if (statusCode == HttpStatusCode.InternalServerError)
                {
                    // Write 500 responses due to technical errors during authentication
                    await ResponseErrorWriter.WriteErrorResponse(
                            this.Request,
                            this.Response,
                            statusCode,
                            clientError.ToResponseFormat());
                }
            }
        }

        /*
         * Get a request item and manage casting
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