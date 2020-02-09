namespace Framework.Api.Base.Security
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Claims;
    using Microsoft.AspNetCore.Http;

    /*
     * An authorizer abstraction
     */
    public interface IAuthorizer
    {
        Task<CoreApiClaims> Execute(HttpRequest request);
    }
}
