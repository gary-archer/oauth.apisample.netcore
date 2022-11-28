namespace SampleApi.Host.Claims
{
    /*
     * User attributes stored in the authorization server
     */
    public sealed class IdentityClaims
    {
        public string Subject { get; set; }

        public string Email { get; set; }
    }
}
