namespace FinalApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Errors;
    using FinalApi.Plumbing.Logging;
    using Jose;

    /*
     * A class to verify the JWT access token, to authenticate the request
     */
    public sealed class AccessTokenValidator
    {
        private readonly OAuthConfiguration configuration;
        private readonly JsonWebKeyResolver jsonWebKeyResolver;
        private readonly ILogEntry logEntry;

        public AccessTokenValidator(
            OAuthConfiguration configuration,
            JsonWebKeyResolver jsonWebKeyResolver,
            ILogEntry logEntry)
        {
            this.configuration = configuration;
            this.jsonWebKeyResolver = jsonWebKeyResolver;
            this.logEntry = logEntry;
        }

        /*
         * Validate the access token using the jose-jwt library
         */
        public async Task<JwtClaims> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("tokenValidator"))
            {
                var claimsJson = string.Empty;
                try
                {
                    // Read the token without validating it, to get its key identifier
                    var kid = this.GetKeyIdentifier(accessToken);
                    if (kid == null)
                    {
                        throw ErrorFactory.CreateClient401Error("Unable to read the kid field from the access token");
                    }

                    // Get the token signing public key as a JSON web key
                    var jwk = await this.jsonWebKeyResolver.GetTokenSigningPublicKey(kid, this.configuration.Algorithm);
                    if (jwk == null)
                    {
                        throw ErrorFactory.CreateClient401Error(
                            $"The token kid {kid} was not found in the JWKS for algorithm {this.configuration.Algorithm}");
                    }

                    // Do the cryptographic validation of the JWT signature using the JWK public key
                    claimsJson = JWT.Decode(accessToken, jwk);
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromTokenValidationError(ex);
                }

                // Save to a claims object
                var claims = new JwtClaims(claimsJson);

                // Verify the protocol claims according to best practices
                this.ValidateProtocolClaims(claims);
                return claims;
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
         * jose-jwt does not support checking standard claims for issuer, audience and expiry, so make those checks here instead
         */
        private void ValidateProtocolClaims(JwtClaims claims)
        {
            // Check the expected issuer
            if (claims.Iss != this.configuration.Issuer)
            {
                throw ErrorFactory.CreateClient401Error("The issuer claim had an unexpected value");
            }

            // Check the expected audience, and Cognito does not issue an audience claim to access tokens
            if (!string.IsNullOrWhiteSpace(this.configuration.Audience))
            {
                var audiences = claims.GetAudiences();
                if (!audiences.Contains(this.configuration.Audience))
                {
                    throw ErrorFactory.CreateClient401Error("The audience claim had an unexpected value");
                }
            }

            // Check that the JWT is not expired
            if (claims.Exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                throw ErrorFactory.CreateClient401Error("The access token is expired");
            }
        }
    }
}
