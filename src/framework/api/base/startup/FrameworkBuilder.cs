namespace Framework.Api.Base.Startup
{
    /*
     * A builder style class to configure framework behaviour and to register its dependencies
     */
    public class FrameworkBuilder
    {

        public FrameworkBuilder Register()
        {
            return this;
        }

        public FrameworkBuilder AddMiddleware()
        {
            return this;
        }
    }
}
