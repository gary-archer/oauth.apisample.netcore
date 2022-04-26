namespace SampleApi.Logic.Entities
{
    /*
     * User info returned to the client for display
     */
    public class ClientUserInfo
    {
        public ClientUserInfo(string givenName, string familyName, string[] regions)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Regions = regions;
        }

        public string GivenName { get; }

        public string FamilyName { get; }

        public string[] Regions { get; }
    }
}