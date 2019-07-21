namespace BasicApi.Utilities
{
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// A utility reader class
    /// </summary>
    public class JsonReader
    {
        /// <summary>
        /// Read JSON text and deserialize it into objects
        /// </summary>
        /// <typeparam name="T">The type of entities to deserialize</typeparam>
        /// <param name="filePath">The JSON file location</param>
        /// <returns>A .Net object of type T</returns>
        public async Task<T> ReadDataAsync<T>(string filePath)
        {
            string jsonText = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(jsonText);
        }
    }
}