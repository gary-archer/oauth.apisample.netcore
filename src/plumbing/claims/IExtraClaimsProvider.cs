namespace FinalApi.Plumbing.Claims
{
    using System;
    using System.Threading.Tasks;

    /*
     * An interface through which OAuth plumbing code calls a repository in the API logic
     */
    public interface IExtraClaimsProvider
    {
        Task<ExtraClaims> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider);
    }
}
