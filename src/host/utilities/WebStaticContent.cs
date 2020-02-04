namespace SampleApi.Host.Utilities
{
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    
    /*
     * For this sample the API also serves web static content, which would not be done by a real API
     * It is expected that web content has been built to parallel folders
     */
    public static class WebStaticContent
    {
        // Root locations of UI code samples that contain web content to serve
        private const string spaRoot = "../authguidance.websample.final/spa";
        private const string desktopRoot = "../authguidance.desktopsample1/web";
        private const string mobileRoot = "../authguidance.mobilesample.android/web";

        /*
         * Define the rules around serving web static content
         */
        public static void Configure(IApplicationBuilder app)
        {
            // Handle custom file resolution when serving static files for the SPA
            app.UseMiddleware<WebStaticContentFileResolver>();

            // This will serve the SPA#s index.html as the default document
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), spaRoot)),
                RequestPath = "/spa"
            });

            // This will serve JS, image and CSS files for the SPA
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), spaRoot)),
                RequestPath = "/spa"
            });

            // Serve post login HTML pages used by our first desktop sample
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), desktopRoot)),
                RequestPath = "/desktop"
            });

            // Serve post login and logout HTML pages used by our Android sample to prevent Chrome Custom Tabs hangs
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), mobileRoot)),
                RequestPath = "/mobile"
            });
        }
    }
}
