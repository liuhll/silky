using System;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Http.Core;
using Silky.Http.MiniProfiler;
using Silky.Swagger.SwaggerGen.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebHostCollectionExtensions
    {
        public static IServiceCollection AddDefaultRegistryCenter(this IServiceCollection services, string registerType)
        {
            switch (registerType.ToLower())
            {
                case "zookeeper":
                    services.AddZookeeperRegistryCenter();
                    break;
                case "nacos":
                    services.AddNacosRegistryCenter();
                    break;
                default:
                    throw new SilkyException(
                        $"The system does not provide a service registration center of type {registerType}");
            }

            return services;
        }

        public static IServiceCollection AddSilkyHttpServices(this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            var redisOptions = EngineContext.Current.Configuration.GetRateLimitRedisOptions();
            services
                .AddSilkyHttpCore()
                .AddClientRateLimit(redisOptions)
                .AddIpRateLimit(redisOptions)
                .AddResponseCaching()
                .AddHttpContextAccessor()
                .AddRouting()
                .AddSilkyIdentity()
                .AddSilkyMiniProfiler()
                .AddSilkyCaching()
                .AddSilkySkyApm()
                .AddSwaggerDocuments(setupAction);
            services.AddMvc();

            return services;
        }
    }
}