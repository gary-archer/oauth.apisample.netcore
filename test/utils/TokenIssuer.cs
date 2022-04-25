namespace SampleApi.Test.Utils
{
    public class TokenIssuer
    {
        public string IssueAccessToken(string subject)
        {
            return $"My token for {subject}";
        }
    }
}