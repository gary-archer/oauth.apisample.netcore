namespace SampleApi.Logic.Entities
{
    /*
     * User info returned to the client for display
     */
    public class ClientUserInfo
    {
        public ClientUserInfo(string role, string[] regions)
        {
            this.Role = role;
            this.Regions = regions;
        }

        public string Role { get; }

        public string[] Regions { get; }
    }
}