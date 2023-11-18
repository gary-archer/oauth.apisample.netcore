namespace SampleApi.Plumbing.Logging
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using log4net.Core;
    using log4net.Layout;

    /*
     * A JSON layout without any log4net generated fields
     */
    internal sealed class JsonLayout : LayoutSkeleton
    {
        private readonly bool prettyPrint;

        public JsonLayout(bool prettyPrint)
        {
            this.prettyPrint = prettyPrint;
        }

        /*
         * An empty implementation of the required overload
         */
        #pragma warning disable S1186
        public override void ActivateOptions()
        {
        }
        #pragma warning restore S1186

        /*
         * Format the output as JSON
         */
        public override void Format(TextWriter writer, LoggingEvent e)
        {
            var data = e.MessageObject as JsonNode;
            if (data != null)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = this.prettyPrint,
                };

                writer.Write(data.ToJsonString(options) + Environment.NewLine);
            }
        }
    }
}
