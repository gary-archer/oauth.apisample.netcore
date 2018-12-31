namespace api.Plumbing
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using api.Entities;
    using api.Plumbing;

    /*
     * A utility reader class
     */
    [Route("api/userclaims")]
    public class JsonReader
    {
        /*
         * Read JSON text and deserialize it
         */
        [HttpGet("current")]
        public async Task<T> ReadDataAsync<T>(string filePath)
        {
            string jsonText = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(jsonText);
        }
    }
}