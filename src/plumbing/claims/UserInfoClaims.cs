namespace SampleApi.Plumbing.Claims
{
    using IdentityModel;
    using Newtonsoft.Json.Linq;

    /*
     * Claims retreived from the user info endpoint
     */
    public class UserInfoClaims
    {
        /*
         * Receive individual claims values
         */
        public UserInfoClaims(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }

        public string GivenName { get; private set; }

        public string FamilyName { get; private set; }

        public string Email { get; private set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static UserInfoClaims ImportData(JObject data)
        {
            var givenName = data.GetValue(JwtClaimTypes.GivenName).Value<string>();
            var familyName = data.GetValue(JwtClaimTypes.FamilyName).Value<string>();
            var email = data.GetValue(JwtClaimTypes.Email).Value<string>();
            return new UserInfoClaims(givenName, familyName, email);
        }

        /*
         * Called when claims are serialized during claims caching
         */
        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.given_name = this.GivenName;
            data.family_name = this.FamilyName;
            data.email = this.Email;
            return data;
        }
    }
}
