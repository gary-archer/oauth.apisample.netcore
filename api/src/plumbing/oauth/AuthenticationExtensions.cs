namespace BasicApi.Plumbing.Utilities
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    /*
     * Extensions related to wiring up custom authentication 
     */
    public static class AuthenticationExtensions
    {
        /*
         * Add our custom handler
         */
        public static AuthenticationBuilder AddCustomHandler(
            this AuthenticationBuilder builder,
            Action<CustomAuthenticationOptions> options)
        {
            // builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OAuth2IntrospectionOptions>, PostConfigureOAuth2IntrospectionOptions>());
            return builder.AddScheme<CustomAuthenticationOptions, CustomAuthenticationHandler>("Bearer", options);
        }
    }
