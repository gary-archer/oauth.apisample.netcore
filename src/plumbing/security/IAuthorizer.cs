namespace SampleApi.Plumbing.Security
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;

    /*
     * An authorizer abstraction that ciould be used for both Entry Point APIs and Microservices
     */
    public interface IAuthorizer
    {
        Task<ClaimsPayload> ExecuteAsync(HttpRequest request);
    }
}
