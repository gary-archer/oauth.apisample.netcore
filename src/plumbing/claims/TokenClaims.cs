namespace SampleApi.Plumbing.Claims
{
    using Newtonsoft.Json.Linq;

    /*
     * Claims received in the access token
     */
    public class TokenClaims
    {
        public TokenClaims(string subject, string clientId, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.ClientId = clientId;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        public string Subject { get; set; }

        public string ClientId { get; set; }

        public string[] Scopes { get; set; }

        public int Expiry { get; set; }

        public static TokenClaims ImportData(JObject data)
        {
            var subject = data.GetValue("subject").Value<string>();
            var clientId = data.GetValue("clientId").Value<string>();
            var scope = data.GetValue("scopes").Value<string>();
            var expiry = data.GetValue("expiry").Value<int>();
            return new TokenClaims(subject, clientId, scope.Split(" "), expiry);
        }

        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.subject = this.Subject;
            data.clientId = this.ClientId;
            data.scopes = string.Join(" ", this.Scopes);
            data.expiry = this.Expiry;
            return data;
        }
    }
}
