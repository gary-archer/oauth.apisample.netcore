namespace SampleApi.Plumbing.Middleware
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Configuration;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Utilities;

    /*
     * A class to process custom headers to enable testers to control non functional behaviour
     */
    public sealed class CustomHeaderMiddleware
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
        public async Task Invoke(HttpContext context, LoggingConfiguration configuration)
        {
            // Cause a 500 error if a special header is received
            var apiToBreak = context.Request.GetHeader("x-mycompany-test-exception");
            if (!string.IsNullOrWhiteSpace(apiToBreak))
            {
                if (apiToBreak.ToLowerInvariant() == configuration.ApiName.ToLowerInvariant())
                {
                    throw ErrorFactory.CreateServerError(ErrorCodes.ExceptionSimulation, "An exception was simulated in the API");
                }
            }

            // Run subsequent handlers including the controller operation
            await this.next(context);
        }
    }
}
