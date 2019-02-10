namespace BasicApi.Plumbing.OAuth
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using BasicApi.Logic;

    /*
    * The entry point for custom authentication
    */
    public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationOptions>
    {
        private readonly ILoggerFactory loggerFactory;

        /*
         * Give the base class the options it needs
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
            // Create authorization related classes on every API request
            var authenticator = new Authenticator(this.Options.OAuthConfiguration, this.Options.IssuerMetadata);
            var rulesRepository = new AuthorizationRulesRepository();
            var claimsMiddleware = new ClaimsMiddleware(
                this.Options.ClaimsCache,
                authenticator,
                rulesRepository,
                this.loggerFactory);

            // Get the access token
            string accessToken = "790245"; //TokenRetrieval.FromAuthorizationHeader()(context.Request);

            // Try to perform the security handling
            var claims = new ApiClaims();
            var success = await claimsMiddleware.authorizeRequestAndSetClaims(accessToken, claims);
            if (success) {

                // On success, set up the .Net security context
                var principal = ClaimsMapper.SerializeToClaimsPrincipal(claims);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            
            // Indicate failure
            return AuthenticateResult.Fail("Authentication Failed");
        }
    }
}