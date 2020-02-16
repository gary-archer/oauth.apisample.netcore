namespace SampleApi.Host.Claims
{
    using Framework.Api.Base.Claims;

    /*
     * Our API overrides the core claims to support additional custom claims
     */
    public class SampleApiClaims : CoreApiClaims
    {
        // Product Specific data used for authorization
        public string[] RegionsCovered { get; set; }
    }
}
