namespace FinalApi.Plumbing.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.Encodings.Web;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Errors;
    using FinalApi.Plumbing.Logging;
    using FinalApi.Plumbing.OAuth;
    using FinalApi.Plumbing.Utilities;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;

    /*
     * A custom authentication filter to take finer control over processing of tokens and claims
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
         * Do the main work to process tokens, claims and log identity details
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Do the authorization work and get a claims principal
                var oauthFilter = (OAuthFilter)this.Context.RequestServices.GetService(typeof(OAuthFilter));
                var claimsPrincipal = await oauthFilter.ExecuteAsync(this.Request);

                // Include selected token details in audit logs
                var userId = claimsPrincipal.Jwt.Sub;
                var scope = claimsPrincipal.Jwt.Scope.Split(" ").ToList();
                var loggedClaims = new JsonObject
                {
                    ["managerId"] = claimsPrincipal.Jwt.GetStringClaim(ClaimNames.ManagerId),
                    ["role"] = claimsPrincipal.Jwt.GetStringClaim(ClaimNames.Role),
                };

                var logEntry = (LogEntry)this.Context.RequestServices.GetService(typeof(ILogEntry));
                logEntry.SetIdentity(userId, scope, loggedClaims);

                // The sample API requires the same scope for all endpoints, and it is enforced here
                var oauthConfiguration = (OAuthConfiguration)this.Context.RequestServices.GetService(typeof(OAuthConfiguration));
                if (!scope.Contains(oauthConfiguration.Scope))
                {
                    throw ErrorFactory.CreateClientError(
                        HttpStatusCode.Forbidden,
                        BaseErrorCodes.InsufficientScope,
                        "The token does not contain sufficient scope for this API");
                }

                // Set up .NET security so that authorization attributes work in the expected way
                var ticket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception exception)
            {
                // Handle 500 errors due to failed processing, which will add details to logs
                var handler = new UnhandledExceptionMiddleware();
                var clientError = handler.HandleException(exception, this.Context);

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
                var oauthConfiguration = (OAuthConfiguration)this.Context.RequestServices.GetService(typeof(OAuthConfiguration));
                await ResponseErrorWriter.WriteErrorResponse(this.Response, clientError, oauthConfiguration.Scope);
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
