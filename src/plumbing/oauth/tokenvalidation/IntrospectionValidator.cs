namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    internal class IntrospectionValidator : ITokenValidator 
    {
        private readonly OAuthConfiguration configuration;

        public IntrospectionValidator(OAuthConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /*
         * The entry point for validating a token via introspection and returning its claims
         */
        public async Task<ClaimsPayload> ValidateTokenAsync(string accessToken)
        {
            try
            {
                using (var client = new HttpClient(this.proxyFactory()))
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

                    return new ClaimsPayload(response);

                    /*
                    // Get token claims
                    string subject = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Subject);
                    string[] scopes = this.GetStringClaim((name) => response.TryGet(name), JwtClaimTypes.Scope).Split(' ');
                    int expiry = this.GetIntegerClaim((name) => response.TryGet(name), JwtClaimTypes.Expiration);

                    // Update token claims
                    return new BaseClaims(subject, scopes, expiry);*/
                }
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromIntrospectionError(ex, this.configuration.IntrospectEndpoint);
            }
        }
    }
}
