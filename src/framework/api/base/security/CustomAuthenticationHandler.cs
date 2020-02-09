namespace Framework.Api.Base.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Framework.Api.Base.Claims;
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
    public sealed class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // Constants used as keys
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
         * Most requests just return cached results from when the token was first processed
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // For secured requests we hit this class before the logger middleware so start logging here
                var logEntry = (LogEntry)this.Context.RequestServices.GetService(typeof(ILogEntry));
                logEntry.Start(this.Request);

                // Do the OAuth work in our plain C# class
                IAuthorizer authorizer = (IAuthorizer)this.Context.RequestServices.GetService(typeof(IAuthorizer));
                var claims = await authorizer.Execute(this.Request);

                // Update the claims holder so that other classes can resolve the claims directly
                var holder = (ClaimsHolder)this.Context.RequestServices.GetService(typeof(ClaimsHolder));
                holder.Value = claims;

                // Add identity details to logs
                logEntry.SetIdentity(claims);

                // Also set up the .Net security context
                var principal = this.CreateClaimsPrincipal(claims);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (ClientError clientError)
            {
                // Handle 401 errors due to invalid tokens
                var handler = new UnhandledExceptionMiddleware();
                handler.HandleException(clientError, this.Context);

                // Store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd(StatusCodeKey, clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd(ClientErrorKey, clientError);
                return AuthenticateResult.NoResult();
            }
            catch (Exception exception)
            {
                // Handle 500 errors dur to failed processing
                var handler = new UnhandledExceptionMiddleware();
                var clientError = handler.HandleException(exception, this.Context);

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
         * We will set up the .Net claims principal here, but will not bother adding all claims to it
         * Our goal is instead to use a strongly typed and technology neutral claims object
         */
        private ClaimsPrincipal CreateClaimsPrincipal(CoreApiClaims claims)
        {
            var claimsList = new List<Claim>();
            claimsList.Add(new Claim(JwtClaimTypes.Subject, claims.UserId));
            var identity = new ClaimsIdentity(claimsList, this.Scheme.Name, JwtClaimTypes.Subject, string.Empty);
            return new ClaimsPrincipal(identity);
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