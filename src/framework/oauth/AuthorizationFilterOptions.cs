namespace Framework.OAuth
{
    using Microsoft.AspNetCore.Authentication;
    using Framework.Configuration;

    /*
     * Custom properties used in our claims handler
     */
    public sealed class AuthorizationFilterOptions : AuthenticationSchemeOptions
    {
        public OAuthConfiguration OAuthConfiguration {get; set;}
    }
}
