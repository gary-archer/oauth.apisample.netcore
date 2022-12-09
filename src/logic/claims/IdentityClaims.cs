namespace SampleApi.Logic.Claims
{
    /*
     * User attributes sent by the authorization server to the API's claims controller
     */
    public sealed class IdentityClaims
    {
        public string Subject { get; set; }

        public string Email { get; set; }
    }
}
