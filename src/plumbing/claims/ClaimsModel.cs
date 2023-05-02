namespace SampleApi.Plumbing.Claims
{
    /*
    * The model into which claims are initially populated
    */
    public class ClaimsModel
    {
        // Protocol claims of interest to the API code
        public string Iss { get; set; }

        public object Aud { get; set; }

        public string Scope { get; set; }

        public long Exp { get; set; }

        public string Sub { get; set; }

        // User info claims
        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string Email { get; set; }

        // Custom claims
        public string UserId { get; set; }

        public string UserRole { get; set; }

        public string[] UserRegions { get; set; }

        public string[] GetAudiences()
        {
            return new string[] { };
        }
    }
}
