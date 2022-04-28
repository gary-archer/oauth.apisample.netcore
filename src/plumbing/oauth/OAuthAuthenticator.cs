namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Jose;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Utilities;

    /*
     * The class from which OAuth calls are initiated
     */
    public sealed class OAuthAuthenticator
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
        public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                try
                {
                    // Read the token without validating it, to get its key identifier
                    var kid = this.GetKeyIdentifier(accessToken);
                    if (kid == null)
                    {
                        var context = "Unable to find a kid in the received access token";
                        throw ErrorFactory.CreateClient401Error(context);
                    }

                    // Get the token signing public key as a JSON web key
                    var jwk = await this.jsonWebKeyResolver.GetKeyForId(kid);
                    if (jwk == null)
                    {
                        var context = "The access token kid was not found";
                        throw ErrorFactory.CreateClient401Error(context);
                    }

                    // Do the cryptographic validation of the JWT signature using the JWK public key
                    var json = JWT.Decode(accessToken, jwk);

                    // Read claims and make extra validation checks that jose4j does not support
                    var claims = ClaimsReader.AccessTokenClaims(json);
                    var identity = this.ValidateClaims(claims);

                    // Return the Microsoft object
                    return new ClaimsPrincipal(identity);
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
        public async Task<IEnumerable<Claim>> GetUserInfoAsync(string accessToken)
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

                        // Convert from JSON data to an array of claims
                        var json = await response.Content.ReadAsStringAsync();
                        return ClaimsReader.UserInfoClaims(json);
                    }
                }
                catch (Exception ex)
                {
                    // Report connectity errors
                    throw ErrorUtils.FromUserInfoError(ex, this.configuration.UserInfoEndpoint);
                }
            }
        }

        /*
         * Read the kid field from the JWT header
         */
        private string GetKeyIdentifier(string accessToken)
        {
            var headers = JWT.Headers(accessToken);
            if (headers.ContainsKey("kid"))
            {
                return headers["kid"] as string;
            }

            return null;
        }

        /*
         * jose-jwt does not support checking standard claims for iss, aud, exp, nbf so we do so here
         */
        private ClaimsIdentity ValidateClaims(IEnumerable<Claim> claims)
        {
            // TODO: validate claims and ensure that the claims identity is in line with the master branch

            // Then return the claims identity
            return new ClaimsIdentity(claims, "Bearer");
        }
    }
}
