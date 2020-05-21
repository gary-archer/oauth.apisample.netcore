namespace SampleApi.Plumbing.OAuth
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;

    /*
     * An authorizer abstraction
     */
    public interface IAuthorizer
    {
        Task<CoreApiClaims> Execute(HttpRequest request);
    }
}
