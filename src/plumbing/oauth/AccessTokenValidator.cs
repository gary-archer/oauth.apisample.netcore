namespace FinalApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Jose;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Configuration;
    using FinalApi.Plumbing.Errors;
    using FinalApi.Plumbing.Logging;

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
                    var jwk = await this.jsonWebKeyResolver.GetKeyForId(kid);
                    if (jwk == null)
                    {
                        throw ErrorFactory.CreateClient401Error($"The token kid {kid} was not found in the JWKS");
                    }

                    // Only accept supported token signing algorithms
                    if (jwk.Alg != this.configuration.Algorithm)
                    {
                        throw ErrorFactory.CreateClient401Error($"The access token algorithm does not have the expected value");
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
            try
            {
                var headers = JWT.Headers(accessToken);
                if (headers.ContainsKey("kid"))
                {
                    return headers["kid"] as string;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromTokenValidationError(ex);
            }
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

            // Check there is a scope
            if (string.IsNullOrWhiteSpace(claims.Scope))
            {
                throw ErrorUtils.FromMissingClaim(OAuthClaimNames.Scope);
            }

            // The sample API requires the same scope for all endpoints, and it is enforced here
            var scopes = claims.Scope.Split(" ");
            if (!scopes.Contains(this.configuration.Scope))
            {
                throw ErrorFactory.CreateClientError(
                    HttpStatusCode.Forbidden,
                    ErrorCodes.InsufficientScope,
                    "The token does not contain sufficient scope for this API");
            }
        }
    }
}
