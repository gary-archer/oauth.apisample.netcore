namespace Framework.OAuth
{
    using Microsoft.AspNetCore.Authentication;
    using Framework.Configuration;

    /// <summary>
    /// Custom properties used in our claims handler
    /// </summary>
    public sealed class AuthorizationFilterOptions : AuthenticationSchemeOptions
    {
        public OAuthConfiguration OAuthConfiguration {get; set;}
    }
}
