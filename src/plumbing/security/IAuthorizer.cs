namespace SampleApi.Plumbing.Security
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /*
     * An authorizer abstraction that ciould be used for both Entry Point APIs and Microservices
     */
    public interface IAuthorizer
    {
        Task<ClaimsPrincipal> ExecuteAsync(HttpRequest request);
    }
}
