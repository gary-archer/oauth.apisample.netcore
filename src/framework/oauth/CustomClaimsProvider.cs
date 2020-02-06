namespace Framework.OAuth
{
    using System.Threading.Tasks;

    /*
     * A base class for adding custom claims from within core claims handling code
     */
    public class CustomClaimsProvider<TClaims>
        where TClaims : CoreApiClaims, new()
    {
        /*
         * This is overridden by base classes
         */
        public virtual Task AddCustomClaimsAsync(string accessToken, TClaims claims)
        {
            return Task.FromResult(0);
        }
    }
}
