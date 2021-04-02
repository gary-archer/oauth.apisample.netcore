namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    public class IntrospectionValidator : ITokenValidator 
    {
        public async Task<ClaimsPayload> ValidateToken(string accessToken)
        {
            try
            {
                using (var client = new HttpClient(this.proxyFactory()))
                {
                    // Send the request
                    var request = new TokenIntrospectionRequest
                    {
                        Address = this.metadata.IntrospectionEndpoint,
                        ClientId = this.configuration.ClientId,
                        ClientSecret = this.configuration.ClientSecret,
                        Token = accessToken,
                    };

                    // Handle errors
                    var response = await client.IntrospectTokenAsync(request);
                    if (response.IsError)
                    {
                        if (response.Exception != null)
                        {
                            throw ErrorUtils.FromIntrospectionError(response.Exception, this.metadata.IntrospectionEndpoint);
                        }
                        else
                        {
                            throw ErrorUtils.FromIntrospectionError(response, this.metadata.IntrospectionEndpoint);
                        }
                    }

                    // Handle invalid or expired tokens
                    if (!response.IsActive)
                    {
                        throw ErrorFactory.CreateClient401Error("Access token is expired and failed introspection");
                    }

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
                throw ErrorUtils.FromIntrospectionError(ex, this.metadata.IntrospectionEndpoint);
            }
        }
    }
}
