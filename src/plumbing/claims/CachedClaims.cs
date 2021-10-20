namespace SampleApi.Plumbing.Claims
{
    /*
     * Claims that are cached between API requests
     */
    public class CachedClaims
    {
        public CachedClaims(UserInfoClaims userInfo, CustomClaims custom)
        {
            this.UserInfo = userInfo;
            this.Custom = custom;
        }

        public UserInfoClaims UserInfo { get; set; }

        public CustomClaims Custom { get; set; }
    }
}
