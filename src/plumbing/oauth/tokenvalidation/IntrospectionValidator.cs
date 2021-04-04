namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using SampleApi.Plumbing.Claims;
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
        public async Task<ClaimsPayload> ValidateTokenAsync(string accessToken)
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

                    var payload = new ClaimsPayload(response);
                    payload.StringClaimCallback = response.TryGet;
                    return payload;
                }
            }
            catch (Exception ex)
            {
                throw ErrorUtils.FromIntrospectionError(ex, this.configuration.IntrospectEndpoint);
            }
        }
    }
}
