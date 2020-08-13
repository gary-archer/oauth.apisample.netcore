namespace SampleApi.Host.Claims
{
    using SampleApi.Plumbing.Claims;

    /*
     * Extend core claims for this particular API
     */
    public class SampleApiClaims : CoreApiClaims
    {
        public string[] RegionsCovered { get; set; }
    }
}
