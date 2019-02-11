namespace BasicApi.Plumbing.OAuth
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using BasicApi.Configuration;
    using BasicApi.Plumbing.Utilities;

    /*
    * Custom properties used in our claims handler
    */
    public class CustomAuthenticationOptions : AuthenticationSchemeOptions
    {
        public OAuthConfiguration OAuthConfiguration {get; set;}

        public IssuerMetadata IssuerMetadata {get; set;}

        public ClaimsCache ClaimsCache {get; set;}

        public Func<ProxyHttpHandler> ProxyHandlerFactory {get; set;}
    }
}
