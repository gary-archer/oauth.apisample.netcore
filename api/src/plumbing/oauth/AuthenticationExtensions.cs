namespace BasicApi.Plumbing.OAuth
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
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
        public static AuthenticationBuilder AddCustomAuthenticationHandler(
            this AuthenticationBuilder builder,
            Action<CustomAuthenticationOptions> options)
        {
            // Register an object to initialize our authentication options after initialization has completed
            // builder.Services.AddSingleton<IPostConfigureOptions<CustomAuthenticationOptions>, PostConfigureAuthenticationOptions>();
            
            // Add our custom authentication handler
            return builder.AddScheme<CustomAuthenticationOptions, CustomAuthenticationHandler>("Bearer", options);
        }
    }
}
