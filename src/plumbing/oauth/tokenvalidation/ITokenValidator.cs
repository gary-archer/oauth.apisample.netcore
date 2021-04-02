namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    internal interface ITokenValidator
    {
        Task<ClaimsPayload> ValidateTokenAsync(string accessToken);
    }
}
