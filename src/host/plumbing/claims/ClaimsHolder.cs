namespace SampleApi.Host.Plumbing.Claims
{
    /*
     * A simple holder class that is request scoped and updated with claims at runtime
     */
    public class ClaimsHolder
    {
        public CoreApiClaims Value { get; set; }
    }
}
