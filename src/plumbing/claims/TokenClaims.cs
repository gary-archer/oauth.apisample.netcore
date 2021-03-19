namespace SampleApi.Plumbing.Claims
{
    using Newtonsoft.Json.Linq;

    /*
     * Claims received in the access token
     */
    public class TokenClaims
    {
        public TokenClaims(string subject, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        public string Subject { get; set; }

        public string[] Scopes { get; set; }

        public int Expiry { get; set; }

        public static TokenClaims ImportData(JObject data)
        {
            var subject = data.GetValue("subject").Value<string>();
            var scope = data.GetValue("scopes").Value<string>();
            var expiry = data.GetValue("expiry").Value<int>();
            return new TokenClaims(subject, scope.Split(" "), expiry);
        }

        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.subject = this.Subject;
            data.scopes = string.Join(" ", this.Scopes);
            data.expiry = this.Expiry;
            return data;
        }
    }
}
