namespace SampleApi.Plumbing.Claims
{
    /*
     * API claims used for authorization
     */
    public class ApiClaims
    {
        public ApiClaims(BaseClaims baseClaims, UserInfoClaims userInfo, CustomClaims custom)
        {
            this.Base = baseClaims;
            this.UserInfo = userInfo;
            this.Custom = custom;
        }

        public BaseClaims Base { get; set; }

        public UserInfoClaims UserInfo { get; set; }

        public CustomClaims Custom { get; set; }
    }
}
