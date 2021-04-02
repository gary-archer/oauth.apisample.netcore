namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System;
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    public class JwtValidator : ITokenValidator 
    {
        public Task<ClaimsPayload> ValidateToken(string accessToken)
        {
            throw new NotImplementedException("not implemented");
        }
    }
}
