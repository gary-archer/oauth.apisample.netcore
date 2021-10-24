namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
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
        private readonly JsonWebKeyResolver jsonWebKeyResolver;
        private readonly HttpProxy httpProxy;
        private readonly LogEntry logEntry;

        public OAuthAuthenticator(
            OAuthConfiguration configuration,
            JsonWebKeyResolver jsonWebKeyResolver,
            HttpProxy httpProxy,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.jsonWebKeyResolver = jsonWebKeyResolver;
            this.httpProxy = httpProxy;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * Validate the access token using the jose-jwt library
         */
        public async Task<JwtPayload> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                try
                {
                    // Read the token without validating it, to get its key identifier
                    var handler = new CustomJwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(accessToken);

                    // Get the token signing public key as a JSON web key
                    var jwk = await this.jsonWebKeyResolver.GetKeyForId(token.Header.Kid);
                    if (jwk == null)
                    {
                        var context = $"The access token kid was not found at the JWKS endpoint";
                        throw ErrorFactory.CreateClient401Error(context);
                    }

                    // Set token validation parameters, and note that Cognito does not provide an audience claim in access tokens
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = jwk,
                        ValidateIssuer = true,
                        ValidIssuer = this.configuration.Issuer,
                        ValidateAudience = string.IsNullOrWhiteSpace(this.configuration.Audience) ? false : true,
                        ValidAudience = this.configuration.Audience,
                    };

                    // The base JwtSecurityTokenHandler checks the above fields and the jose library validates the signature
                    SecurityToken result;
                    handler.ValidateToken(accessToken, tokenValidationParameters, out result);
                    var jwt = result as JwtSecurityToken;
                    return jwt.Payload;
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromTokenValidationError(ex);
                }
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
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var request = new HttpRequestMessage(HttpMethod.Get, this.configuration.UserInfoEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        var response = await client.SendAsync(request);

                        // Report errors with a response
                        if (!response.IsSuccessStatusCode)
                        {
                            var status = (int)response.StatusCode;
                            var text = await response.Content.ReadAsStringAsync();
                            throw ErrorUtils.FromUserInfoError(status, text, this.configuration.UserInfoEndpoint);
                        }

                        // Return the claims
                        var json = await response.Content.ReadAsStringAsync();
                        return ClaimsReader.UserInfoClaims(JObject.Parse(json));
                    }
                }
                catch (Exception ex)
                {
                    // Report connectity errors
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }
    }
}
