using JWT.Algorithms;

namespace Silky.Jwt.Configuration
{
    public class JwtOptions
    {
        internal static string JwtSettings = "JwtSettings";

        public JwtOptions()
        {
            Algorithm = JwtAlgorithmName.HS256;
            Issuer = "http://silky.com/issuer";
            Audience = "http://silky.com/audience";
            ExpiredTime = 24;
        }

        public string Secret { get; set; }

        public JwtAlgorithmName Algorithm { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ExpiredTime { get; set; }
    }
}