namespace SampleApi.Plumbing.Claims
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

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

        /*
         * Serialize to an output format that we can control
         */
        public string Serialize(ApiClaims claims)
        {
            dynamic data = new JObject();
            data.token = claims.Token.ExportData();
            data.userInfo = claims.UserInfo.ExportData();
            data.custom = claims.Custom.ExportData();
            return data.ToString();
        }

        /*
         * Deserialize in a manner that enables us to control the type of object
         */
        public ApiClaims Deserialize(string claimsText)
        {
            var data = JObject.Parse(claimsText);
            var token = TokenClaims.ImportData(data.GetValue("token").Value<JObject>());
            var userInfo = UserInfoClaims.ImportData(data.GetValue("userInfo").Value<JObject>());
            var custom = this.DeserializeCustomClaims(data.GetValue("custom").Value<JObject>());
            return new ApiClaims(token, userInfo, custom);
        }

        /*
         * This default implementation can be overridden to manage deserialization
         */
        protected virtual CustomClaims DeserializeCustomClaims(JObject claimsNode)
        {
            return CustomClaims.ImportData(claimsNode);
        }
    }
}
