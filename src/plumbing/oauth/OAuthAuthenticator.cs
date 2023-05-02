namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Jose;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;

    /*
     * A class to verify the JWT access token, to authenticate the request
     */
    public sealed class OAuthAuthenticator
    {
        private readonly OAuthConfiguration configuration;
        private readonly JsonWebKeyResolver jsonWebKeyResolver;
        private readonly ILogEntry logEntry;

        public OAuthAuthenticator(
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
        public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken)
        {
            using (this.logEntry.CreatePerformanceBreakdown("userInfoLookup"))
            {
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
                    if (jwk.Alg != "RS256")
                    {
                        throw ErrorFactory.CreateClient401Error($"The access token kid was not found in the JWKS");
                    }

                    // Do the cryptographic validation of the JWT signature using the JWK public key
                    var claimsJson = JWT.Decode(accessToken, jwk);

                    // Deserialize to an object
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy(),
                        },
                    };
                    var claimsModel = JsonConvert.DeserializeObject<ClaimsModel>(claimsJson, settings);

                    // Read claims and create the Microsoft objects so that .NET logic can use the standard mechanisms
                    var claims = ClaimsReader.BaseClaims(claimsJson, this.configuration);
                    var identity = new ClaimsIdentity(claims, "Bearer");
                    var principal = new ClaimsPrincipal(identity);

                    // Make extra validation checks that jose4j does not support, then return the principal
                    this.ValidateProtocolClaims(claimsModel);
                    return principal;
                }
                catch (Exception ex)
                {
                    throw ErrorUtils.FromTokenValidationError(ex);
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
         * jose-jwt does not support checking standard claims for issuer, audience and expiry, so make those checks here instead
         */
        private void ValidateProtocolClaims(ClaimsModel claimsModel)
        {
            if (claimsModel.Iss != this.configuration.Issuer)
            {
                throw ErrorFactory.CreateClient401Error("The issuer claim had an unexpected value");
            }

            if (!string.IsNullOrWhiteSpace(this.configuration.Audience))
            {
                var audiences = claimsModel.GetAudiences();
                if (!audiences.Contains(this.configuration.Audience))
                {
                    throw ErrorFactory.CreateClient401Error("The audience claim had an unexpected value");
                }
            }

            if (claimsModel.Exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                throw ErrorFactory.CreateClient401Error("The access token is expired");
            }
        }
    }
}
