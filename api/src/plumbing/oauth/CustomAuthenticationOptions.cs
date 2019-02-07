namespace BasicApi.Plumbing.OAuth
{
    using Microsoft.AspNetCore.Authentication;
    using BasicApi.Plumbing.Utilities;

    /*
    * Custom properties used in our claims handler
    */
    public class CustomAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Authority {get; set;}

        public string ClientId {get; set;}

        public string ClientSecret {get; set;}

        public string NameClaimType {get; set;}

        public string RoleClaimType {get; set;}

        public ProxyHttpHandler ProxyHttpHandler {get; set;}
    }
}
