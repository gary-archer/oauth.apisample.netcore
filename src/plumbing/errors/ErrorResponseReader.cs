namespace SampleApi.Plumbing.Errors
{
    using System;
    using Newtonsoft.Json.Linq;

    /*
     * A utility to read JSON and ignore errors, to make the calling code simpler
     * This prevents 'double faults' during error handling
     */
    internal static class ErrorResponseReader
    {
        public static JObject ReadJson(string jsonText)
        {
            if (jsonText == null || jsonText.Length == 0)
            {
                return null;
            }

            try
            {
                return JObject.Parse(jsonText);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}