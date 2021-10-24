namespace SampleApi.Plumbing.OAuth
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Cryptography;
    using Jose;
    using Microsoft.IdentityModel.Tokens;

    /*
     * A class to customize cryptographic validation of JWTs using a jose library
     */
    internal class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        /*
         * The Jose library could use more advanced algorithms such as PS256 if needed
         */
        protected override JwtSecurityToken ValidateSignature(string token, TokenValidationParameters validationParameters)
        {
            // Get the key retrieved earlier in the authenticator class
            var jwk = validationParameters.IssuerSigningKey as JsonWebKey;

            // We are using RS256 so convert the jwk to an RSA public key object as required by the jose library
            var rsaKey = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsaKey.ImportParameters(new RSAParameters
            {
                Modulus = Base64Url.Decode(jwk.N),
                Exponent = Base64Url.Decode(jwk.E),
            });

            // Do the cryptographic validation of the JWT signature
            Jose.JWT.Decode(token, rsaKey);

            // Pass the token to the base Microsoft handler, which will make additional checks
            return new JwtSecurityToken(token);
        }
    }
}