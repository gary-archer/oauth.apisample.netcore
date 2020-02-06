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
            // TODO: Add the child container
            System.Console.WriteLine("*** Creating the child container for this request");

            // Run the next handler
            await this.next(context);
        }
    }
}
