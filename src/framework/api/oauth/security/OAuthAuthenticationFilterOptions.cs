namespace Framework.Api.OAuth.Security
{
    using Framework.Api.OAuth.Configuration;
    using Microsoft.AspNetCore.Authentication;

    /*
     * Properties used by our custom .Net Core authentication filter
     */
    public sealed class OAuthAuthenticationFilterOptions : AuthenticationSchemeOptions
    {
        public OAuthConfiguration Configuration { get; set; }
    }
}
