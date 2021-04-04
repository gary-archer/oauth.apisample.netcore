namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.IdentityModel.Tokens;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Utilities;

    /*
     * An implementation that validates access tokens as JWTs
     */
    internal sealed class JwtValidator : ITokenValidator
    {
        private readonly OAuthConfiguration configuration;
        private readonly HttpProxy httpProxy;

        public JwtValidator(OAuthConfiguration configuration, HttpProxy httpProxy)
        {
            this.configuration = configuration;
            this.httpProxy = httpProxy;
        }

        public async Task<ClaimsPayload> ValidateTokenAsync(string accessToken)
        {
            // Next get the token signing public key
            var keys = await this.GetTokenSigningPublicKeysAsync();

            // Next validate the token
            var principal = this.ValidateJsonWebToken(accessToken, keys);

            // Return the claims for processing later
            return new ClaimsPayload(principal);

            /*// Get token claims, and note that Microsoft move the subject claim to a username field
            string subject = this.GetStringClaim((name) => principal.FindFirstValue(name), "username");
            string[] scopes = this.GetStringClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Scope).Split(' ');
            int expiry = this.GetIntegerClaim((name) => principal.FindFirstValue(name), JwtClaimTypes.Expiration);

            // Update token claims
            return new BaseClaims(subject, scopes, expiry);*/
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
                    ValidateAudience = true,
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
    }
}
