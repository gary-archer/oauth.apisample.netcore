namespace SampleApi.Plumbing.Claims
{
    /*
     * API claims used for authorization
     */
    public class ApiClaims
    {
        public TokenClaims Token { get; set; }

        public UserInfoClaims UserInfo { get; set; }

        public CustomClaims Custom { get; set; }

        public ApiClaims(TokenClaims token, UserInfoClaims userInfo, CustomClaims custom)
        {
            this.Token = token;
            this.UserInfo = userInfo;
            this.Custom = custom;
        }
    }
}
