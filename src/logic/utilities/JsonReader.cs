namespace SampleApi.Logic.Utilities
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
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
                return JsonConvert.DeserializeObject<T>(jsonText);
            }
            catch (Exception ex)
            {
                // Report the error including an error code and exception details
                var error = ErrorFactory.CreateServerError(
                    SampleErrorCodes.FileReadError,
                    "Problem encountered reading data",
                    ex);
                error.SetDetails(ex.Message);
                throw error;
            }
        }
    }
}