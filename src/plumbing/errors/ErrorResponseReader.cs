namespace SampleApi.Plumbing.Errors
{
    using System;
    using System.Text.Json.Nodes;

    /*
     * A utility to read JSON and ignore errors, to make the calling code simpler
     * This prevents potential 'double faults' during error handling
     */
    internal static class ErrorResponseReader
    {
        public static JsonNode ReadJson(string jsonText)
        {
            if (jsonText == null || jsonText.Length == 0)
            {
                return null;
            }

            try
            {
                return JsonNode.Parse(jsonText);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
