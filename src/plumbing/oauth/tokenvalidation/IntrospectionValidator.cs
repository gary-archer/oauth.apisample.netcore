namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Utilities;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    internal sealed class IntrospectionValidator : ITokenValidator
    {
        private readonly OAuthConfiguration configuration;
        private readonly HttpProxy httpProxy;

        public IntrospectionValidator(OAuthConfiguration configuration, HttpProxy httpProxy)
        {
            this.configuration = configuration;
            this.httpProxy = httpProxy;
        }

        /*
         * The entry point for validating a token via introspection and returning its claims
         */
        public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken)
        {
            try
            {
                using (var client = new HttpClient(this.httpProxy.GetHandler()))
                {
                    // Send the request
                    var request = new TokenIntrospectionRequest
                    {
                        Address = this.configuration.IntrospectEndpoint,
                        ClientId = this.configuration.IntrospectClientId,
                        ClientSecret = this.configuration.IntrospectClientSecret,
                        Token = accessToken,
                    };

                    // Handle errors
                    var response = await client.IntrospectTokenAsync(request);
                    if (response.IsError)
                    {
                        if (response.Exception != null)
                        {
                            throw ErrorUtils.FromIntrospectionError(response.Exception, this.configuration.IntrospectEndpoint);
                        }
                        else
                        {
                            throw ErrorUtils.FromIntrospectionError(response, this.configuration.IntrospectEndpoint);
                        }
                    }

                    // Handle invalid or expired tokens
                    if (!response.IsActive)
                    {
                        throw ErrorFactory.CreateClient401Error("Access token is expired and failed introspection");
                    }

                    // Read values into a claims principal
                    var subject = this.GetClaim(response, JwtClaimTypes.Subject);
                    var scope = this.GetClaim(response, JwtClaimTypes.Scope);
                    var expiry = this.GetClaim(response, JwtClaimTypes.Expiration);
                    return this.CreateClaimsPrincipal(subject, scope, expiry);
                }
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromIntrospectionError(ex, this.configuration.IntrospectEndpoint);
            }
        }

        /*
         * Read a claim and report missing errors clearly
         */
        public string GetClaim(TokenIntrospectionResponse response, string name)
        {
            var value = response.TryGet(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        /*
         * Create a claims principal from the introspection results
         */
        public ClaimsPrincipal CreateClaimsPrincipal(string subject, string scope, string expiry)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtClaimTypes.Subject, subject));
            claims.Add(new Claim(JwtClaimTypes.Scope, scope));
            claims.Add(new Claim(JwtClaimTypes.Expiration, expiry));

            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }
    }
}
