namespace Framework.Api.Base.Logging
{
    using System;
    using System.IO;
    using log4net.Core;
    using log4net.Layout;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        public override void ActivateOptions()
        {
        }

        /*
         * Format the output as JSON
         */
        public override void Format(TextWriter writer, LoggingEvent e)
        {
            var data = e.MessageObject as JObject;
            if (data != null)
            {
                // Console output is indented and file output is a single object per line
                var formatting = this.prettyPrint ? Formatting.Indented : Formatting.None;

                // Write the data
                writer.Write(data.ToString(formatting) + Environment.NewLine);
            }
        }
    }
}