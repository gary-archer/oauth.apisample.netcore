namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
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
        public async Task<JObject> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("validateToken"))
            {
                try
                {
                    // Read the kid field from the JWT header
                    var headers = Jose.JWT.Headers(accessToken);
                    var kid = headers["kid"]?.ToString();
                    if (string.IsNullOrWhiteSpace(kid))
                    {
                        var context = $"The access token had no kid in its JWT header";
                        throw ErrorFactory.CreateClient401Error(context);
                    }

                    // Get the token signing public key as a JSON web key
                    var jwk = await this.jsonWebKeyResolver.GetKeyForId(kid);

                    // Convert to an RSA public key object as required by the library
                    var rsaKey = new System.Security.Cryptography.RSACryptoServiceProvider();
                    rsaKey.ImportParameters(new RSAParameters
                    {
                        Modulus = Base64Url.Decode(jwk.N),
                        Exponent = Base64Url.Decode(jwk.E),
                    });

                    // Do the token validation and return the claims in a generic security object
                    var claims = Jose.JWT.Decode(accessToken, rsaKey);
                    return JObject.Parse(claims);
                }
                catch (Exception ex)
                {
                    // Handle failures and log the error details
                    var details = $"JWT verification failed: ${ex.Message}";
                    throw ErrorFactory.CreateClient401Error(details);
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

                        return ClaimsReader.UserInfoClaims(response);
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
