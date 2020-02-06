namespace Framework.OAuth
{
    using Framework.Configuration;
    using Microsoft.AspNetCore.Authentication;

    /*
     * Custom properties used in our claims handler
     */
    public sealed class AuthorizationFilterOptions : AuthenticationSchemeOptions
    {
        public OAuthConfiguration OAuthConfiguration { get; set; }
    }
}
