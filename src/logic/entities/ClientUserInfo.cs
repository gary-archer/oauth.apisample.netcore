namespace SampleApi.Logic.Entities
{
    /*
     * User info from the API's own data, returned to clients for display
     */
    public class ClientUserInfo
    {
        public ClientUserInfo(string title, string[] regions)
        {
            this.Title = title;
            this.Regions = regions;
        }

        public string Title { get; }

        public string[] Regions { get; }
    }
}