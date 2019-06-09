namespace Framework.Utilities
{
    using Microsoft.AspNetCore.Authentication;
    using Framework.OAuth;

    /// <summary>
    /// Helper methods for setting up authentication
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// An extension method to register our custom authentication handler
        /// </summary>
        /// <typeparam name="TClaims"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static AuthorizationFilterBuilder<TClaims> AddCustomAuthorizationFilter<TClaims>(
            this AuthenticationBuilder builder, AuthorizationFilterOptions options) 
                where TClaims: CoreApiClaims, new()
        {
            // Add the handler
            builder.AddScheme<AuthorizationFilterOptions, AuthorizationFilter<TClaims>>("Bearer", (o) => {});

            // Return a custom builder object to make usage of common code easier in a concrete API
            return new AuthorizationFilterBuilder<TClaims>(options);
        }
    }
}
