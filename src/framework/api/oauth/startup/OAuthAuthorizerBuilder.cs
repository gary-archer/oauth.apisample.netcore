namespace Framework.Api.OAuth.Startup
{
    using Framework.Api.Base.Security;
    using Framework.Api.OAuth.Claims;

    /*
     * Build a simple authorizer for receiving claims via headers
     */
    public class OAuthAuthorizerBuilder<TClaims>
        where TClaims : CoreApiClaims, new()
    {
        /*
         * Register and return the authorizer
         */
        public BaseAuthorizer Register()
        {
            return new BaseAuthorizer();
        }
    }
}