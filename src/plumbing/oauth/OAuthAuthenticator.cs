namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.IdentityModel.Tokens;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Utilities;

    /*
     * The class from which OAuth calls are initiated
     */
    internal sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly HttpProxy httpProxy;
        private readonly LogEntry logEntry;

        public OAuthAuthenticator(
            OAuthConfiguration configuration,
            HttpProxy httpProxy,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
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
                // Get the token signing public key
                var keys = await this.GetTokenSigningPublicKeysAsync();

                // Use it to validate the token and read a claims principal
                return this.ValidateJsonWebToken(accessToken, keys);
            }
        }

        /*
         * Perform OAuth user info lookup
         */
        public async Task<UserInfoClaims> GetUserInfoAsync(string accessToken)
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
                        return new UserInfoClaims(givenName, familyName, email);
                    }
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }

        /*
         * Get the keys from the JWKS endpoint
         */
        private async Task<string> GetTokenSigningPublicKeysAsync()
        {
            try
            {
                using (var client = new HttpClient(this.httpProxy.GetHandler()))
                {
                    // Make the HTTPS request
                    var response = await client.GetJsonWebKeySetAsync(this.configuration.JwksEndpoint);
                    if (response.IsError)
                    {
                        if (response.Exception != null)
                        {
                            throw ErrorUtils.FromTokenSigningKeysDownloadError(response.Exception, this.configuration.JwksEndpoint);
                        }
                        else
                        {
                            throw ErrorUtils.FromTokenSigningKeysDownloadError(response, this.configuration.JwksEndpoint);
                        }
                    }

                    // Return the JSON data
                    return response.Json.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromTokenSigningKeysDownloadError(ex, this.configuration.JwksEndpoint);
            }
        }

        /*
        * Do the work of verifying the access token
        */
        private ClaimsPrincipal ValidateJsonWebToken(string accessToken, string keys)
        {
            try
            {
                // Set parameters, and Cognito does not provide an audience claim in access tokens
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = this.configuration.Issuer,
                    IssuerSigningKeys = new JsonWebKeySet(keys).Keys,
                    ValidateAudience = string.IsNullOrWhiteSpace(this.configuration.Audience) ? false : true,
                    ValidAudience = this.configuration.Audience,
                    ValidAlgorithms = new[] { "RS256" },
                };

                // Do the technical validation, including checking the digital signature via the public key
                var handler = new JwtSecurityTokenHandler();
                SecurityToken result;
                return handler.ValidateToken(accessToken, tokenValidationParameters, out result);
            }
            catch (Exception ex)
            {
                // Handle failures and log the error details
                var details = $"JWT verification failed: ${ex.Message}";
                throw ErrorFactory.CreateClient401Error(details);
            }
        }

        /*
         * Read a claim and report missing errors clearly
         */
        private string GetClaim(UserInfoResponse response, string name)
        {
            var value = response.TryGet(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }
    }
}
