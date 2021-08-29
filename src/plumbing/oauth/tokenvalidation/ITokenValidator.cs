namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    internal interface ITokenValidator
    {
        Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken);
    }
}
