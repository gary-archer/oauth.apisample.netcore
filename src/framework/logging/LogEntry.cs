namespace Framework.Logging
{
    using Newtonsoft.Json.Linq;

    /*
     * A basic log entry object with a per request scope
     */
    public class LogEntry
    {
        private JObject error;

        public LogEntry()
        {
            System.Console.WriteLine("Creating Log Entry for a Request");
            this.error = null;
        }

        /*
         * An empty implementation of the required overload
         */
        public void AddError(JObject error)
        {
            this.error = error;
        }

        /*
         * Output the data
         */
        public void Write()
        {
        }
    }
}