namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using BasicApi.Logic;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.Utilities;

    /*
     * An instance of this class is created for every API request, which then runs our introspection and claims handling
     */
    public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationOptions>
    {
        /*
         * Injected dependencies
         */
        private readonly ILoggerFactory loggerFactory;
        
        /*
         * The base class requires the first four parameters
         */
        public CustomAuthenticationHandler(
            IOptionsMonitor<CustomAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock): base(options, loggerFactory, urlEncoder, clock)
        {
            this.loggerFactory = loggerFactory;
        }

        /*
         * This is called once per API request to perform authorization
         */
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Create authorization related classes on every API request
                var authenticator = new Authenticator(this.Options.OAuthConfiguration, this.Options.IssuerMetadata);
                var rulesRepository = new AuthorizationRulesRepository();
                var claimsMiddleware = new ClaimsMiddleware(
                    this.Options.ClaimsCache,
                    authenticator,
                    rulesRepository,
                    this.loggerFactory);

                // Get the access token
                string accessToken = this.ReadAccessToken();

                // Try to perform the security handling
                var claims = new ApiClaims();
                var success = await claimsMiddleware.authorizeRequestAndSetClaims(accessToken, claims);
                if (success) {

                    // On success, set up the .Net security context
                    var principal = ClaimsMapper.SerializeToClaimsPrincipal(claims);
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
                var handler = new ErrorHandler();
                var clientError = handler.HandleError(exception, this.loggerFactory.CreateLogger<CustomAuthenticationHandler>());
                
                // Next store fields for the challenge method which will fire later
                this.Request.HttpContext.Items.TryAdd("statusCode", clientError.StatusCode);
                this.Request.HttpContext.Items.TryAdd("clientError", clientError);
                return AuthenticateResult.NoResult();
            }
        }

        /*
         * Write authentication errors if required, after the above handler has completed
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
                            (int)statusCode,
                            ((ClientError)clientError).ToResponseFormat());
                }
            }
        }

        /*
         * Try to read the bearer header
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