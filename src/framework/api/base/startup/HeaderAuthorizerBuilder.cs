namespace Framework.Api.Base.Startup
{
    using Framework.Api.Base.Security;

    /*
     * Build a simple authorizer for receiving claims via headers
     */
    public class HeaderAuthorizerBuilder
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
