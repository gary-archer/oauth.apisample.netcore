namespace SampleApi.Test.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Jose;

    /*
     * A token issuer for testing
     */
    public class TokenIssuer : IDisposable
    {
        private readonly string algorithm;
        private readonly RSA rsa;
        private readonly Jwk tokenSigningPrivateKey;
        private readonly Jwk tokenSigningPublicKey;
        private readonly string keyId;

        /*
         * Create keys for testing
         */
        public TokenIssuer()
        {
            this.algorithm = "RS256";
            this.keyId = Guid.NewGuid().ToString();

            this.rsa = RSA.Create(2048);

            this.tokenSigningPrivateKey = new Jwk(this.rsa, true);
            this.tokenSigningPrivateKey.Alg = this.algorithm;
            this.tokenSigningPrivateKey.KeyId = this.keyId;

            this.tokenSigningPublicKey = new Jwk(this.rsa, false);
            this.tokenSigningPublicKey.Alg = this.algorithm;
            this.tokenSigningPublicKey.KeyId = this.keyId;
        }

        /*
         * Get the token signing public keys as a JSON Web Keyset
         */
        public string GetTokenSigningPublicKeys()
        {
            var keyset = new JwkSet(this.tokenSigningPublicKey);
            return keyset.ToJson(JWT.DefaultSettings.JsonMapper);
        }

        /*
         * Issue an access token with the supplied subject claim
         */
        public string IssueAccessToken(string subject)
        {
            var now = DateTimeOffset.Now;
            var iat = now.AddMinutes(-30);
            var exp = now.AddMinutes(30);

            var payload = new Dictionary<string, object>()
            {
                { "sub", subject },
                { "iss", "testissuer.com" },
                { "aud", "api.mycompany.com" },
                { "iat", iat.ToUnixTimeSeconds() },
                { "exp", exp.ToUnixTimeSeconds() },
                { "scope", "openid profile email https://api.authsamples.com/api/transactions_read" },
            };

            return Jose.JWT.Encode(payload, this.tokenSigningPrivateKey, JwsAlgorithm.RS256);
        }

        /*
         * Issue an access token signed with an untrusted JWK
         */
        public string IssueMaliciousAccessToken(string subject)
        {
            using (var maliciousRSA = RSA.Create(2048))
            {
                var maliciousPrivateKey = new Jwk(maliciousRSA, true);
                maliciousPrivateKey.Alg = this.algorithm;
                maliciousPrivateKey.KeyId = this.keyId;

                var now = DateTimeOffset.Now;
                var iat = now.AddMinutes(-30);
                var exp = now.AddMinutes(30);

                var payload = new Dictionary<string, object>()
                {
                    { "sub", subject },
                    { "iss", "testissuer.com" },
                    { "aud", "api.mycompany.com" },
                    { "iat", iat.ToUnixTimeSeconds() },
                    { "exp", exp.ToUnixTimeSeconds() },
                    { "scope", "openid profile email https://api.authsamples.com/api/transactions_read" },
                };

                return Jose.JWT.Encode(payload, maliciousPrivateKey, JwsAlgorithm.RS256);
            }
        }

        /*
         * Dispose the internal key
         */
        public void Dispose()
        {
            this.rsa.Dispose();
        }
    }
}
