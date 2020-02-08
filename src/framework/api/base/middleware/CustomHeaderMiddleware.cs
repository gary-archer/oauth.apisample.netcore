namespace Framework.Api.Base.Middleware
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Configuration;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Utilities;
    using Microsoft.AspNetCore.Http;

    /*
     * A class to process custom headers to enable testers to control non functional behaviour
     */
    internal sealed class CustomHeaderMiddleware
    {
        private readonly RequestDelegate next;

        /*
         * Store a reference to the next middleware
         */
        public CustomHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /*
         * Handle any special custom headers
         */
        public async Task Invoke(HttpContext context, FrameworkConfiguration configuration)
        {
            // Cause a 500 error if a special header is received
            var apiToBreak = context.Request.GetHeader("x-mycompany-test-exception");
            if (!string.IsNullOrWhiteSpace(apiToBreak))
            {
                if (apiToBreak.ToLowerInvariant() == configuration.ApiName.ToLowerInvariant())
                {
                    throw ErrorFactory.CreateApiError(BaseErrorCodes.ExceptionSimulation, "An exception was simulated in the API");
                }
            }

            // Run subsequent handlers including the controller operation
            await this.next(context);
        }
    }
}
