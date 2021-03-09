namespace SampleApi.Plumbing.Claims
{
    using Newtonsoft.Json.Linq;

    /*
     * Claims retreived from the user info endpoint
     */
    public class UserInfoClaims
    {
        public UserInfoClaims(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }

        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string Email { get; set; }

        public static UserInfoClaims ImportData(JObject data)
        {
            var givenName = data.GetValue("givenName").Value<string>();
            var familyName = data.GetValue("familyName").Value<string>();
            var email = data.GetValue("email").Value<string>();
            return new UserInfoClaims(givenName, familyName, email);
        }

        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.givenName = this.GivenName;
            data.familyName = this.FamilyName;
            data.email = this.Email;
            return data;
        }
    }
}
