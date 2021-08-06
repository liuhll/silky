using Autofac;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            

            services.AddTransient<IJsonSerializer, JsonNetSerializer>();
            services.AddTransient<IBase64UrlEncoder, JwtBase64UrlEncoder>();
            services.AddTransient<IJwtEncoder, JwtEncoder>();
            services.AddTransient<IJwtDecoder, JwtDecoder>();
            services.AddTransient<IJwtValidator, JwtValidator>();
            services.AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            
            builder.RegisterType<ES256Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.ES256.ToString());
            
            builder.RegisterType<ES384Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.ES384.ToString());
            
            builder.RegisterType<ES512Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.ES512.ToString());
            
            builder.RegisterType<HMACSHA256Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.HS256.ToString());
            
            builder.RegisterType<HMACSHA384Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.HS384.ToString());
            
            builder.RegisterType<HMACSHA512Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.HS512.ToString());
            
            builder.RegisterType<RS256Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.RS256.ToString());
            
            builder.RegisterType<RS384Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.RS384.ToString());
            
            builder.RegisterType<RS512Algorithm>()
                .Named<IJwtAlgorithm>(JwtAlgorithmName.RS512.ToString());
        }
    }
}