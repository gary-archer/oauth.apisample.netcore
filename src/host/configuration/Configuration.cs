namespace FinalApi.Host.Configuration
{
    using System.Threading.Tasks;
    using FinalApi.Logic.Utilities;
    using FinalApi.Plumbing.Configuration;

    /*
     * A class to manage our JSON configuration as an object
     */
    public class Configuration
    {
        // The API's specific configuration
        public ApiConfiguration Api { get; set; }

        // Common logging
        public LoggingConfiguration Logging { get; set; }

        // Common OAuth processing
        public OAuthConfiguration OAuth { get; set; }

        /*
         * A utility method to load the file and deal with casing
         */
        public static Task<Configuration> LoadAsync(string filePath)
        {
            var reader = new JsonReader();
            return reader.ReadDataAsync<Configuration>(filePath);
        }
    }
}
