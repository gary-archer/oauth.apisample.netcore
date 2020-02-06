namespace Framework.Utilities
{
    using Framework.OAuth;
    using Microsoft.AspNetCore.Authentication;

    /*
     * Helper methods for setting up authentication
     */
    public static class AuthenticationExtensions
    {
        /*
         * An extension method to register our custom authentication handler
         */
        public static AuthorizationFilterBuilder<TClaims> AddCustomAuthorizationFilter<TClaims>(
            this AuthenticationBuilder builder, AuthorizationFilterOptions options)
                where TClaims : CoreApiClaims, new()
        {
            // Add the handler
            builder.AddScheme<AuthorizationFilterOptions, AuthorizationFilter<TClaims>>("Bearer", (o) => { });

            // Return a custom builder object to make usage of common code easier in a concrete API
            return new AuthorizationFilterBuilder<TClaims>(options);
        }
    }
}
