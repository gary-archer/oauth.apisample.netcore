namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using IdentityModel;
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
        #pragma warning disable 1998
        protected virtual async Task<CustomClaims> SupplyCustomClaimsAsync(
            ClaimsPayload tokenData,
            ClaimsPayload userInfoData)
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

        /*
         * Read base claims from the supplied token data
         */
        private BaseClaims ReadBaseClaims(ClaimsPayload data)
        {
            var subject = data.GetStringClaim(JwtClaimTypes.Subject);
            var scopes = data.GetStringClaim(JwtClaimTypes.Scope).Split(' ');
            var expiry = Convert.ToInt32(data.GetStringClaim(JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);
            return new BaseClaims(subject, scopes, expiry);
        }

        /*
         * Read user info claims from the supplied data, which could originate from a token or user info payload
         */
        private UserInfoClaims ReadUserInfoClaims(ClaimsPayload data)
        {
            var givenName = data.GetStringClaim(JwtClaimTypes.GivenName);
            var familyName = data.GetStringClaim(JwtClaimTypes.FamilyName);
            var email = data.GetStringClaim(JwtClaimTypes.Email);
            return new UserInfoClaims(givenName, familyName, email);
        }
    }
}
