namespace SampleApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    /*
     * The model into which claims are initially deserialized
    */
    public class ClaimsModel
    {
        // Protocol claims of interest to the API code
        public string Iss { get; set; }

        public object Aud { private get; set; }

        public string Scope { get; set; }

        public int Exp { get; set; }

        public string Sub { get; set; }

        // User info claims
        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string Email { get; set; }

        // Custom claims
        public string UserId { get; set; }

        public string UserRole { get; set; }

        public object UserRegions { private get; set; }

        /*
         * Return audiences as an array
         */
        public IEnumerable<string> GetAudiences()
        {
            return this.ObjectToArray(this.Aud);
        }

        /*
         * Return the custom regions claim as an array
         */
        public IEnumerable<string> GetUserRegions()
        {
            return this.ObjectToArray(this.UserRegions);
        }

        /*
         * Handle optional arrays of strings
         */
        private IEnumerable<string> ObjectToArray(object data)
        {
            var results = new List<string>();

            if (data is string)
            {
                results.Add(data as string);
            }

            if (data is JArray)
            {
                var audiences = data as JArray;
                foreach (var audience in audiences)
                {
                    results.Add(audience.Value<string>());
                }
            }

            return results;
        }
    }
}
