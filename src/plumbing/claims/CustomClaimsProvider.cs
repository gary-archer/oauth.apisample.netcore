namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /*
     * A default custom claims provider implementation
     */
    public class CustomClaimsProvider
    {
        public ApiClaims ReadClaims(ClaimsPayload tokenData)
        {
            return new ApiClaims(
                this.ReadBaseClaims(tokenData),
                this.ReadUserInfoClaims(tokenData),
                this.ReadCustomClaims(tokenData));

        }

        /*
         * This is overridden by base classes
         */
        public async Task<ApiClaims> SupplyClaimsAsync(ClaimsPayload tokenData, ClaimsPayload userInfoData)
        {
            var customClaims = await this.SupplyCustomClaimsAsync(tokenData, userInfoData);

            return new ApiClaims(
                this.ReadBaseClaims(tokenData),
                this.ReadUserInfoClaims(userInfoData),
                customClaims);
        }

        /*
         * Serialize to an output format that we can control
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
         * Deserialize in a manner that enables us to control the type of object
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
        protected virtual CustomClaims ReadCustomClaims(ClaimsPayload payload)
        {
            return new CustomClaims();
        }

        /*
         * This default implementation can be overridden by derived classes
         */
        protected virtual async Task<CustomClaims> SupplyCustomClaimsAsync(
            ClaimsPayload tokenData,
            ClaimsPayload userInfoData)
        {
            return new CustomClaims();
        }

        /*
         * This default implementation can be overridden to manage deserialization
         */
        protected virtual CustomClaims DeserializeCustomClaims(JObject claimsNode)
        {
            return CustomClaims.ImportData(claimsNode);
        }

        /*
         * Read base claims from the supplied token data
         */
        private BaseClaims ReadBaseClaims(ClaimsPayload data)
        {
            /*const subject = data.getClaim('sub');
            const scopes = data.getClaim('scope').split(' ');
            const expiry = parseInt(data.getClaim('exp'), 10);
            return new BaseClaims(subject, scopes, expiry);*/

            throw new NotImplementedException("not implemented");
        }

        /*
         * Read user info claims from the supplied data, which could originate from a token or user info payload
         */
        private UserInfoClaims ReadUserInfoClaims(ClaimsPayload data)
        {
            /*
            const givenName = data.getClaim('given_name');
            const familyName = data.getClaim('family_name');
            const email = data.getClaim('email');
            return new UserInfoClaims(givenName, familyName, email);*/

            throw new NotImplementedException("not implemented");
        }
    }
}
