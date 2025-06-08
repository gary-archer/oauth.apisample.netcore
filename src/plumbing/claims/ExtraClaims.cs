namespace FinalApi.Plumbing.Claims
{
    using System.Collections.Generic;

    /*
     * Represents extra authorization values not received in access tokens
     */
    public class ExtraClaims
    {
        /*
         * The default constructor for deserialization
         */
        public ExtraClaims()
        {
            this.Title = string.Empty;
            this.Regions =
                [];
        }

        /*
         * Construct with values from the API's own data
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
