namespace SampleApi.Host.Errors
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /*
     * Create a child container per request
     */
    public class ChildContainerMiddleware
    {
        private readonly RequestDelegate next;

        /*
         * Store a reference to the next middleware
         */
        public ChildContainerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /*
         * Create the child container
         */
        public async Task Invoke(HttpContext context)
        {
            System.Console.WriteLine("*** Child container middleware");

            // Run the next handler
            await this.next(context);
        }
    }
}
