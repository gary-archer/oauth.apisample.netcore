namespace SampleApi.Plumbing.Claims
{
    using System.Threading.Tasks;

    /*
     * A base class for enabling custom claims to be included in the cache after OAuth processing
     */
    public class CustomClaimsProvider
    {
        /*
         * This is overridden by base classes
         */
        public virtual Task<CustomClaims> GetCustomClaimsAsync(TokenClaims token, UserInfoClaims userInfo)
        {
            var claims = new CustomClaims();
            return Task.FromResult(claims);
        }
    }
}
