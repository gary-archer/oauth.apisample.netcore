namespace SampleApi.Plumbing.Claims
{
    using Newtonsoft.Json.Linq;

    /*
     * Can be overridden to provide custom claims from the API's own data
     */
    public class CustomClaims
    {
        public static CustomClaims ImportData(JObject claimsText)
        {
            return new CustomClaims();
        }

        public virtual JObject ExportData()
        {
            return new JObject();
        }
    }
}
