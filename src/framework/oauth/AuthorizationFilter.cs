namespace Framework.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Framework.Errors;
    using Framework.Utilities;

    /*
     * An instance of this class is created for every API request
     */
    public sealed class AuthorizationFilter<TClaims> : AuthenticationHandler<AuthorizationFilterOptions>
        where TClaims : CoreApiClaims, new()
    {
        private readonly ClaimsMiddleware<TClaims> claimsMiddleware;
        private readonly ILoggerFactory loggerFactory;

        public AuthorizationFilter(
            IOptionsMonitor<AuthorizationFilterOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock,
            ClaimsMiddleware<TClaims> claimsMiddleware): base(options, loggerFactory, urlEncoder, clock)
        {
            this.claimsMiddleware = claimsMiddleware;
            this.loggerFactory = loggerFactory;
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Get the access token
                string accessToken = this.ReadAccessToken();

                // Try to perform the security handling
                TClaims claims = await this.claimsMiddleware.authorizeRequestAndGetClaims(accessToken);
                if (claims != null) {

                    // Get claims for the claims principal
                    var claimsList = new List<Claim>();
                    claims.WriteToPrincipal(claimsList);

                    // Set up the .Net security context
                    var identity = new ClaimsIdentity(claimsList, Scheme.Name, JwtClaimTypes.Subject, string.Empty);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                else {
                    
                    // For 401 responses we set a status for the challenge method which will fire later
                    this.Request.HttpContext.Items.TryAdd("statusCode", 401);
                    return AuthenticateResult.NoResult();
                }
            }
            catch (Exception exception)
            {
                // For 500 responses we first log the error
                var handler = new OAuthErrorHandler();
                var logger = this.loggerFactory.CreateLogger<AuthorizationFilter<TClaims>>();
                var clientError = handler.HandleError(exception, logger);
                
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
                if(clientError != null)
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
         * Try to read the bearer token from the authorization header
         */
        private string ReadAccessToken()
        {
            string authorization = this.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0] == "Bearer") 
                {
                    return parts[1];
                }
            }

            return null;
        }

        /*
         * Get a request item and manage casting 
         */
        private T GetRequestItem<T>(string name)
        {
            var item = this.Request.HttpContext.Items[name];
            if(item != null)
            {
                return (T)item;
            }

            return default(T);
        }
    }
}