namespace SampleApi.Logic.Utilities
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using SampleApi.Logic.Errors;
    using SampleApi.Plumbing.Errors;

    /*
     * A utility reader class
     */
    public class JsonReader
    {
        /*
         * Read JSON text and deserialize it into objects
         */
        public async Task<T> ReadDataAsync<T>(string filePath)
        {
            try
            {
                string jsonText = await File.ReadAllTextAsync(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                return JsonSerializer.Deserialize<T>(jsonText, options);
            }
            catch (Exception ex)
            {
                // Report the error including an error code and exception details
                throw ErrorFactory.CreateServerError(
                    SampleErrorCodes.FileReadError,
                    "Problem encountered reading data",
                    ex);
            }
        }
    }
}
