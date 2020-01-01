namespace SampleApi.Host.Utilities
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /*
     * A utility to customise how static content is served
     */
    public class WebStaticContentFileResolver
    {
        private readonly RequestDelegate next;

        public WebStaticContentFileResolver(RequestDelegate next)
        {
            this.next = next;
        }

        /*
         * When the web configuration file is requested, we serve the local API version
         */
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().ToLower().Contains("spa.config.json"))
            {
                context.Request.Path = new PathString("/spa/spa.config.localapi.json");
            }

            await this.next(context);
        }
    }
}