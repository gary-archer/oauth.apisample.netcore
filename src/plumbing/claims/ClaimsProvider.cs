namespace SampleApi.Plumbing.Claims
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /*
     * The claims provider class is responsible for providing the final claims object
     */
    public class ClaimsProvider
    {
        /*
         * Used with the standard authorizer to read all claims from the access token
         */
        public ApiClaims ReadClaims(ClaimsPrincipal tokenData)
        {
            return new ApiClaims(
                new BaseClaims(tokenData),
                new UserInfoClaims(tokenData),
                this.ReadCustomClaims(tokenData));
        }

        /*
         * Used with the claims caching authorizer to gather claims from various sources
         */
        public async Task<ApiClaims> SupplyClaimsAsync(ClaimsPrincipal tokenData, ClaimsPrincipal userInfoData)
        {
            var customClaims = await this.SupplyCustomClaimsAsync(tokenData, userInfoData);

            return new ApiClaims(
                new BaseClaims(tokenData),
                new UserInfoClaims(userInfoData),
                customClaims);
        }

        /*
         * Called when claims are serialized during claims caching
         */
        public string SerializeToCache(ApiClaims claims)
        {
            dynamic data = new JObject();
            data.token = claims.Base.ExportData();
            data.userInfo = claims.UserInfo.ExportData();
            data.custom = claims.Custom.ExportData();
            return data.ToString();
        }

        /*
         * Called when claims are deserialized during claims caching
         */
        public ApiClaims DeserializeFromCache(string claimsText)
        {
            var data = JObject.Parse(claimsText);
            var token = BaseClaims.ImportData(data.GetValue("token").Value<JObject>());
            var userInfo = UserInfoClaims.ImportData(data.GetValue("userInfo").Value<JObject>());
            var custom = this.DeserializeCustomClaims(data.GetValue("custom").Value<JObject>());
            return new ApiClaims(token, userInfo, custom);
        }

        /*
         * This default implementation can be overridden by derived classes
         */
        protected virtual CustomClaims ReadCustomClaims(ClaimsPrincipal principal)
        {
            return new CustomClaims();
        }

        /*
         * This default implementation can be overridden by derived classes
         */
        #pragma warning disable 1998
        protected virtual async Task<CustomClaims> SupplyCustomClaimsAsync(
            ClaimsPrincipal tokenData,
            ClaimsPrincipal userInfoData)
        {
            return new CustomClaims();
        }
        #pragma warning restore 1998

        /*
         * This default implementation can be overridden to manage deserialization
         */
        protected virtual CustomClaims DeserializeCustomClaims(JObject claimsNode)
        {
            return CustomClaims.ImportData(claimsNode);
        }
    }
}
