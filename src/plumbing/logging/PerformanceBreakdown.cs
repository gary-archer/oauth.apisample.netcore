namespace FinalApi.Plumbing.Logging
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json.Nodes;

    /*
     * Performs basic timing of expensive subtasks
     */
    internal sealed class PerformanceBreakdown : IPerformanceBreakdown
    {
        private readonly string name;
        private readonly Stopwatch stopWatch;
        private readonly IList<PerformanceBreakdown> children;
        private JsonNode details;

        public PerformanceBreakdown(string name)
        {
            this.name = name;
            this.stopWatch = new Stopwatch();
            this.children = new List<PerformanceBreakdown>();
            this.MillisecondsTaken = 0;
            this.details = null;
        }

        /*
         * Return the milliseconds as an integer
         */
        public int MillisecondsTaken { get; set; }

        /*
         * Start a performance measurement after creation
         */
        public void Start()
        {
            this.stopWatch.Start();
        }

        /*
        * Set details to associate with the performance breakdown, such as SQL and parameters
        */
        public void SetDetails(JsonNode value)
        {
            this.details = value;
        }

        /*
        * Stop the timer and finish the measurement, converting nanoseconds to milliseconds
        */
        public void Dispose()
        {
            this.stopWatch.Stop();
            this.MillisecondsTaken = (int)this.stopWatch.ElapsedMilliseconds;
        }

        /*
        * Return data as an object
        */
        public JsonNode GetData()
        {
            var data = new JsonObject()
            {
                ["name"] = this.name,
                ["millisecondsTaken"] = this.MillisecondsTaken,
            };

            if (this.details != null)
            {
                data["details"] = this.details;
            }

            if (this.children.Count > 0)
            {
                data["children"] = new JsonArray(this.children.Select(c => c.GetData()).ToArray());
            }

            return data;
        }

        /*
        * Add a child to the performance breakdown
        */
        public IPerformanceBreakdown CreateChild(string name)
        {
            var child = new PerformanceBreakdown(name);
            this.children.Add(child);
            child.Start();
            return child;
        }
    }
}
