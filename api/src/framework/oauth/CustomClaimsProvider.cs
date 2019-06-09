namespace Framework.OAuth
{
    using System.Threading.Tasks;

    /// <summary>
    /// A base class for adding custom claims from within core claims handling code
    /// </summary>
    public class CustomClaimsProvider<TClaims> where TClaims : CoreApiClaims, new()
    {
        /// <summary>
        /// The method to add claims
        /// </summary>
        /// <param name="accessToken">The access token</param>
        /// <param name="claims">The claims to update</param>
        /// <returns>A task to await</returns>
        public virtual Task AddCustomClaimsAsync(string accessToken, TClaims claims)
        {
            return Task.FromResult(0);
        }
    }
}
