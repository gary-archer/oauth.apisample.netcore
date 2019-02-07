namespace BasicApi.Plumbing.OAuth
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /*
    * Our custom handler class
    */
    public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationOptions>
    {
        /*
         * Give the base class the options it needs
         */
        public CustomAuthenticationHandler(
            IOptionsMonitor<CustomAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock): base(options, loggerFactory, urlEncoder, clock)
        {
        }

        /*
         * The main authentication option
         */
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // TODO: Create hard coded working claims
            var claims = new List<Claim>();

            // Create the security context
            var id = new ClaimsIdentity(claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType);
            var principal = new ClaimsPrincipal(id);
            var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);

            // Return the result
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}