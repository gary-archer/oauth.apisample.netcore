namespace SampleApi.Plumbing.OAuth.TokenValidation
{
    using System.Threading.Tasks;
    using SampleApi.Plumbing.Claims;

    /*
     * An interface for validating tokens, which can have multiple implementations
     */
    public interface ITokenValidator
    {
        Task<ClaimsPayload> ValidateToken(string accessToken);
    }
}
