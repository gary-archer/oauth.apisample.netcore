namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.OAuth.TokenValidation;

    /*
     * The class from which OAuth calls are initiated
     */
    internal sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly ITokenValidator tokenValidator;
        private readonly Func<HttpClientHandler> proxyFactory;
        private readonly LogEntry logEntry;

        public OAuthAuthenticator(
            OAuthConfiguration configuration,
            ITokenValidator tokenValidator,
            Func<HttpClientHandler> proxyFactory,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.tokenValidator = tokenValidator;
            this.proxyFactory = proxyFactory;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * The entry point for validating an access token
         */
        public async Task<ClaimsPayload> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
                return await this.tokenValidator.ValidateTokenAsync(accessToken);
            }
        }

        /*
         * Perform OAuth user info lookup
         */
        public async Task<ClaimsPayload> GetUserInfoAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
                try
                {
                    using (var client = new HttpClient(this.proxyFactory()))
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

                        return new ClaimsPayload(response);

                        /*// Get token claims and use the immutable user id as the subject claim
                        string givenName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.GivenName);
                        string familyName = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.FamilyName);
                        string email = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Email);
                        return new UserInfoClaims(givenName, familyName, email);*/
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }

        /*
         * A helper to get a string claim
         */
        private string GetStringClaim(Func<string, string> callback, string name)
        {
            var value = callback(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        /*
         * A helper to get an integer claim
         */
        private int GetIntegerClaim(Func<string, string> callback, string name)
        {
            var value = callback(name);
            if (value == null)
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
    }
}
