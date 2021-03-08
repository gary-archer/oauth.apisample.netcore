namespace SampleApi.Plumbing.Claims
{
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
    }
}
