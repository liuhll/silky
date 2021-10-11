using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Jwt.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtServiceCollectionExtensions
    {
        public static IServiceCollection AddJwt(this IServiceCollection services)
        {
            services.AddOptions<JwtOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(JwtOptions.JwtSettings));

            var jwtAlgorithm =
                EngineContext.Current.Configuration.GetValue<JwtAlgorithmName?>("jwtSettings:algorithm") ??
                JwtAlgorithmName.HS256;
            services.AddTransient<IJsonSerializer, JsonNetSerializer>();
            services.AddTransient<IBase64UrlEncoder, JwtBase64UrlEncoder>();
            services.AddTransient<IJwtEncoder, JwtEncoder>();
            services.RegisterJwtAlgorithm(jwtAlgorithm);
            services.AddTransient<IJwtValidator, JwtValidator>();
            services.AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
            services.AddTransient<IJwtDecoder, JwtDecoder>();
            return services;
        }

        private static void RegisterJwtAlgorithm(this IServiceCollection services, JwtAlgorithmName jwtAlgorithmName)
        {
            switch (jwtAlgorithmName)
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
        }
    }
}