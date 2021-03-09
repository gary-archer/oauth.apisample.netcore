namespace SampleApi.Logic.Entities
{
    /*
     * User info returned to the client for display
     */
    public class ClientUserInfo
    {
        public ClientUserInfo(string givenName, string familyName)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
        }

        public string GivenName { get; }

        public string FamilyName { get; }
    }
}