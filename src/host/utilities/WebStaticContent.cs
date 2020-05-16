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
        private const string SpaRoot = "../authguidance.websample.final/spa";
        private const string LoopbackRoot = "../authguidance.desktopsample1/web";
        private const string DesktopRoot = "../authguidance.desktopsample.final/web";
        private const string AndroidRoot = "../authguidance.mobilesample.android/web";
        private const string IosRoot = "../authguidance.mobilesample.ios/web";

        /*
         * Define the rules around serving web static content
         */
        public static void Configure(IApplicationBuilder app)
        {
            // Handle custom file resolution when serving static files for the SPA
            app.UseMiddleware<WebStaticContentFileResolver>();

            // This will serve the SPA's index.html as the default document
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), SpaRoot)),
                RequestPath = "/spa",
            });

            // This will serve JS, image and CSS files for the SPA
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), SpaRoot)),
                RequestPath = "/spa",
            });

            // Serve post login HTML pages used by our first desktop sample
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), LoopbackRoot)),
                RequestPath = "/loopback",
            });

            // Serve post login HTML pages used by our final desktop sample
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), DesktopRoot)),
                RequestPath = "/desktop",
            });

            // Serve post login and logout interstitial web pages used by our Android sample
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), AndroidRoot)),
                RequestPath = "/android",
            });

            // Serve post login and logout interstitial web pages used by our iOS sample
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CustomPhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), IosRoot)),
                RequestPath = "/ios",
            });
        }
    }
}
