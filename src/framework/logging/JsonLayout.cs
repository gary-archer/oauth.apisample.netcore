namespace Framework.Logging
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
    public class JsonLayout : LayoutSkeleton
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
                var formatting = this.prettyPrint ? Formatting.Indented : Formatting.None;
                writer.Write(data.ToString(formatting) + Environment.NewLine);
            }
        }
    }
}