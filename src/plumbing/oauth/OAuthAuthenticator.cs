namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Claims;
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
        public async Task<ClaimsPayload> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
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

                        var payload = new ClaimsPayload(response);
                        payload.StringClaimCallback = response.TryGet;
                        return payload;
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }
    }
}
