namespace BasicApi.Utilities
{
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

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
            string jsonText = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(jsonText);
        }
    }
}