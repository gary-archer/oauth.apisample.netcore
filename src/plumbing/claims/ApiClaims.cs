namespace SampleApi.Plumbing.Claims
{
    /*
     * API claims used for authorization
     */
    public class ApiClaims
    {
        public ApiClaims(TokenClaims token, UserInfoClaims userInfo, CustomClaims custom)
        {
            this.Token = token;
            this.UserInfo = userInfo;
            this.Custom = custom;
        }

        public TokenClaims Token { get; set; }

        public UserInfoClaims UserInfo { get; set; }

        public CustomClaims Custom { get; set; }
    }
}
