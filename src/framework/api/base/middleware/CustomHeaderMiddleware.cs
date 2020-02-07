namespace Framework.Api.Base.Middleware
{
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Framework.Api.Base.Logging;
    using Microsoft.AspNetCore.Http;

    /*
     * A class to process custom headers to enable testers to control non functional behaviour
     */
    public class CustomHeaderMiddleware
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
        public async Task Invoke(HttpContext context, LogEntry logEntry)
        {
            // Cause a 500 error if a special header is received
            var key = "x-mycompany-test-exception";
            if (context.Request.Headers.ContainsKey(key))
            {
                throw new ApiError("exception_simulation", "An exception was simulated in the API");
            }

            // Run subsequent handlers including the controller operation
            await this.next(context);
        }
    }
}
