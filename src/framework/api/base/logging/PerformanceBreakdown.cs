namespace Framework.Api.Base.Logging
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Framework.Base.Abstractions;
    using Newtonsoft.Json.Linq;

    /*
     * The full implementation class is private to the framework and excluded from the index.ts file
     */
    internal sealed class PerformanceBreakdown : IPerformanceBreakdown
    {
        private readonly string name;
        private Stopwatch stopWatch;
        private string details;
        private IList<PerformanceBreakdown> children;

        public PerformanceBreakdown(string name)
        {
            this.name = name;
            this.stopWatch = new Stopwatch();
            this.children = new List<PerformanceBreakdown>();
            this.MillisecondsTaken = 0;
            this.details = string.Empty;
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
        * Set details to associate with the performance breakdown
        * One use case would be to log SQL with input parameters
        */
        public void SetDetails(string value)
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
        public JObject GetData()
        {
            dynamic data = new JObject();
            data.name = this.name;
            data.millisecondsTaken = this.MillisecondsTaken;

            if (this.details.Length > 0)
            {
                data.details = this.details;
            }

            if (this.children.Count > 0)
            {
                data.children = new JArray();
                foreach (var child in this.children)
                {
                    data.children.Add(child.GetData());
                }
            }

            return data;
        }

        /*
        * Add a child to the performance breakdown
        */
        public PerformanceBreakdown CreateChild(string name)
        {
            var child = new PerformanceBreakdown(name);
            this.children.Add(child);
            return child;
        }
    }
}
