namespace SampleApi.Test.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Jose;

    public class TokenIssuer
    {
        private readonly string algorithm;
        private readonly Jwk tokenSigningPrivateKey;
        private readonly Jwk tokenSigningPublicKey;
        private readonly string keyId;

        public TokenIssuer()
        {
            this.algorithm = "RS256";
            this.keyId = Guid.NewGuid().ToString();

            using (var rsa = RSA.Create(2048))
            {
                this.tokenSigningPrivateKey = new Jwk(rsa.ExportRSAPrivateKey());
                this.tokenSigningPrivateKey.Alg = this.algorithm;
                //this.tokenSigningPrivateKey.Kty = "RSA";
                this.tokenSigningPrivateKey.KeyId = this.keyId;
                
                this.tokenSigningPublicKey = new Jwk(rsa.ExportRSAPublicKey());
                this.tokenSigningPublicKey.Alg = this.algorithm;
                //this.tokenSigningPublicKey.Kty = "RSA";
                this.tokenSigningPublicKey.KeyId = this.keyId;
            }
        }

        public string GetTokenSigningPublicKeys()
        {
            var keyset = new JwkSet(this.tokenSigningPublicKey);
            return keyset.ToJson(JWT.DefaultSettings.JsonMapper);
        }

        public string IssueAccessToken(string subject)
        {
            var payload = new Dictionary<string, object>()
            {
                { "sub", subject },
                { "iss", "testissuer.com" },
                { "aud", "api.mycompany.com" },
                { "exp", "whatevar" }, // FIX
                { "scope", "openid profile email https://api.authsamples.com/api/transactions_read" }
            };

            return Jose.JWT.Encode(payload, this.tokenSigningPrivateKey, JwsAlgorithm.RS256);

            /*
            .setProtectedHeader( { kid: this._keyId, alg: this._algorithm } )
            .setIssuedAt(now - 30000)
            .setExpirationTime(now + 30000)
            */
        }

        public string IssueMaliciousAccessToken(string subject)
        {
            using (var rsa = RSA.Create(2048))
            {
                var maliciousPrivateKey = new Jwk(rsa.ExportRSAPrivateKey());
                
                var maliciousPublicKey = new Jwk(rsa.ExportRSAPublicKey());
                maliciousPublicKey.Alg = this.algorithm;
                maliciousPublicKey.KeyId = this.keyId;
            
                var payload = new Dictionary<string, object>()
                {
                    { "sub", subject },
                    { "iss", "testissuer.com" },
                    { "aud", "api.mycompany.com" },
                    { "exp", "whatevar" }, // FIX
                    { "scope", "openid profile email https://api.authsamples.com/api/transactions_read" }
                };

                return Jose.JWT.Encode(payload, this.tokenSigningPrivateKey, JwsAlgorithm.RS256);
            }
        }
    }
}