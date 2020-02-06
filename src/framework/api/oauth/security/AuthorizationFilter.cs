namespace Framework.Api.OAuth.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Framework.Api.Base.Utilities;
    using Framework.Api.OAuth.Claims;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;

    /*
     * An instance of this class is created for every API request and manages Microsoft specific classes
     */
    public sealed class AuthorizationFilter<TClaims> : AuthenticationHandler<AuthorizationFilterOptions>
        where TClaims : CoreApiClaims, new()
    {
        private readonly Authorizer<TClaims> authorizer;

        public AuthorizationFilter(
            IOptionsMonitor<AuthorizationFilterOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock,
            Authorizer<TClaims> authorizer)
                : base(options, loggerFactory, urlEncoder, clock)
        {
            this.authorizer = authorizer;
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Perform the security handling to get claims
                TClaims claims = await this.authorizer.Execute(this.Request);

                // Get claims into a collection
                var claimsList = new List<Claim>();
                claims.Output(claimsList);

                // Set up the .Net security context
                var identity = new ClaimsIdentity(claimsList, this.Scheme.Name, JwtClaimTypes.Subject, string.Empty);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (ClientError)
            {
                // Handle 401 responses
                this.Request.HttpContext.Items.TryAdd("statusCode", 401);
                return AuthenticateResult.NoResult();
            }
            catch (Exception exception)
            {
                // Handle 500 responses by first logging the error
                var handler = new OAuthErrorHandler();
                var logEntry = new LogEntry();
                var clientError = handler.HandleError(exception, logEntry);
                logEntry.End(this.Response);

                // Next store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd("statusCode", clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd("clientError", clientError);
                return AuthenticateResult.NoResult();
            }
        }

        /*
         * This returns any authentication error responses to the API caller
         */
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var statusCode = this.GetRequestItem<int>("statusCode");
            if (statusCode == 401)
            {
                // Write 401 responses due to invalid tokens
                await ResponseErrorWriter.WriteInvalidTokenResponse(this.Request, this.Response);
            }
            else if (statusCode == 500)
            {
                // Write 500 responses due to technical errors during authentication
                var clientError = this.GetRequestItem<ClientError>("clientError");
                if (clientError != null)
                {
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
        private T GetRequestItem<T>(string name)
        {
            var item = this.Request.HttpContext.Items[name];
            if (item != null)
            {
                return (T)item;
            }

            return default(T);
        }
    }
}