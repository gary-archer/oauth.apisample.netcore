namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.OAuth.TokenValidation;
    using SampleApi.Plumbing.Utilities;

    /*
     * The class from which OAuth calls are initiated
     */
    internal sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly ITokenValidator tokenValidator;
        private readonly HttpProxy httpProxy;
        private readonly LogEntry logEntry;

        public OAuthAuthenticator(
            OAuthConfiguration configuration,
            ITokenValidator tokenValidator,
            HttpProxy httpProxy,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.tokenValidator = tokenValidator;
            this.httpProxy = httpProxy;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                return await this.tokenValidator.ValidateTokenAsync(accessToken);
            }
        }

        /*
         * Perform OAuth user info lookup
         */
        public async Task<ClaimsPrincipal> GetUserInfoAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
                try
                {
                    using (var client = new HttpClient(this.httpProxy.GetHandler()))
                    {
                        // Send the request
                        var request = new UserInfoRequest
                        {
                            Address = this.configuration.UserInfoEndpoint,
                            Token = accessToken,
                        };
                        var response = await client.GetUserInfoAsync(request);

                        // Handle errors
                        if (response.IsError)
                        {
                            // Handle technical errors
                            if (response.Exception != null)
                            {
                                throw ErrorUtils.FromUserInfoError(response.Exception, this.configuration.UserInfoEndpoint);
                            }
                            else
                            {
                                throw ErrorUtils.FromUserInfoError(response, this.configuration.UserInfoEndpoint);
                            }
                        }

                        // Read values into a claims principal
                        var givenName = this.GetClaim(response, JwtClaimTypes.GivenName);
                        var familyName = this.GetClaim(response, JwtClaimTypes.FamilyName);
                        var email = this.GetClaim(response, JwtClaimTypes.Email);
                        return this.CreateClaimsPrincipal(givenName, familyName, email);
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }

        /*
         * Read a claim and report missing errors clearly
         */
        public string GetClaim(UserInfoResponse response, string name)
        {
            var value = response.TryGet(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        /*
         * Create a claims principal from the user info results
         */
        public ClaimsPrincipal CreateClaimsPrincipal(string givenName, string familyName, string email)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtClaimTypes.GivenName, givenName));
            claims.Add(new Claim(JwtClaimTypes.FamilyName, familyName));
            claims.Add(new Claim(JwtClaimTypes.Email, email));

            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }
    }
}
