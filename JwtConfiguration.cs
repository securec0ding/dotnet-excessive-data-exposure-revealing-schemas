using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend
{
    public static class JwtConfiguration
    {
        public const string SigningKey = "defaultsecretkey";
        public const string Audience = "Hunter2 BadSecretKey App";
        public const string Issuer = "Hunter2 BadSecretKey App";
        public const int ExpireSeconds = int.MaxValue;

        public static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                RequireExpirationTime = false,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            };
        }

        public static byte[] SigningKeyBytes
        {
            get
            {
                return Encoding.UTF8.GetBytes(SigningKey);
            }
        }
    }
}