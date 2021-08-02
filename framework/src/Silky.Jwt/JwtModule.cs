using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core.Modularity;
using Silky.Jwt.Configuration;

namespace Silky.Jwt
{
    public class JwtModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.JwtSettings));
            var serviceProvider = services.BuildServiceProvider();
            var jwtOptions = serviceProvider.GetService<IOptions<JwtOptions>>().Value;
            switch (jwtOptions.Algorithm)
            {
                case JwtAlgorithmName.ES256:
                    services.AddTransient<IJwtAlgorithm, ES256Algorithm>();
                    break;
                case JwtAlgorithmName.ES384:
                    services.AddTransient<IJwtAlgorithm, ES384Algorithm>();
                    break;
                case JwtAlgorithmName.ES512:
                    services.AddTransient<IJwtAlgorithm, ES512Algorithm>();
                    break;
                case JwtAlgorithmName.HS256:
                    services.AddTransient<IJwtAlgorithm, HMACSHA256Algorithm>();
                    break;
                case JwtAlgorithmName.HS384:
                    services.AddTransient<IJwtAlgorithm, HMACSHA384Algorithm>();
                    break;
                case JwtAlgorithmName.HS512:
                    services.AddTransient<IJwtAlgorithm, HMACSHA512Algorithm>();
                    break;
                case JwtAlgorithmName.RS256:
                    services.AddTransient<IJwtAlgorithm, RS256Algorithm>();
                    break;
                case JwtAlgorithmName.RS384:
                    services.AddTransient<IJwtAlgorithm, RS384Algorithm>();
                    break;
                case JwtAlgorithmName.RS512:
                    services.AddTransient<IJwtAlgorithm, RS512Algorithm>();
                    break;
            }

            services.AddTransient<IJsonSerializer, JsonNetSerializer>();
            services.AddTransient<IBase64UrlEncoder, JwtBase64UrlEncoder>();
            services.AddTransient<IJwtEncoder, JwtEncoder>();
            services.AddTransient<IJwtDecoder, JwtDecoder>();
            services.AddTransient<IJwtValidator, JwtValidator>();
            services.AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
        }
    }
}