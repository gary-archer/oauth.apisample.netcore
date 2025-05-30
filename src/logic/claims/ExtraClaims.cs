namespace FinalApi.Logic.Claims
{
    using System.Collections.Generic;

    /*
     * Represents extra claims not received in access tokens
     */
    public class ExtraClaims
    {
        /*
         * The default constructor
         */
        public ExtraClaims()
        {
            this.Title = string.Empty;
            this.Regions = new string[] { };
        }

        /*
         * Construct with claims that are always looked up from the API's own data
         */
        public ExtraClaims(string title, string[] regions)
        {
            this.Title = title;
            this.Regions = regions;
        }

        public string Title { get; set; }

        public IEnumerable<string> Regions { get; set; }
    }
}
