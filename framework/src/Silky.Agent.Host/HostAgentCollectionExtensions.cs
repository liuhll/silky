using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.RegistryCenter.Consul;
using Silky.Swagger.Abstraction.SwaggerGen.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostAgentCollectionExtensions
    {
        internal static IServiceCollection AddDefaultRegistryCenter(this IServiceCollection services,
            string registerType, IConfiguration configuration)
        {
            switch (registerType.ToLower())
            {
                case "zookeeper":
                    services.AddZookeeperRegistryCenter();
                    break;
                case "nacos":
                    services.AddNacosRegistryCenter();
                    break;
                case "consul":
                    services.AddConsulRegistryCenter();
                    break;
                default:
                    throw new SilkyException(
                        $"The system does not provide a service registration center of type {registerType}");
            }

            return services;
        }

        public static IServiceCollection AddSilkyHttpServices(this IServiceCollection services,
            Action<SwaggerGenOptions> swaggerOptionAction = null,
            Action<AuthorizationOptions> authorizationOptionAction = null)
        {
            var redisOptions = EngineContext.Current.Configuration.GetRateLimitRedisOptions();
            services
                .AddSilkyHttpCore()
                .AddClientRateLimit(redisOptions)
                .AddIpRateLimit(redisOptions)
                .AddResponseCaching()
                .AddHttpContextAccessor()
                .AddRouting()
                .AddSilkyIdentity(authorizationOptionAction)
                .AddSilkyMiniProfiler()
                .AddSilkyCaching()
                .AddDashboard()
                .AddSwaggerDocuments(swaggerOptionAction);
            services.AddMvc();

            return services;
        }

        public static IServiceCollection AddSilkyHttpServices<TAuthorizationHandler>(this IServiceCollection services,
            Action<SwaggerGenOptions> swaggerOptionAction = null,
            Action<AuthorizationOptions> authorizationOptionAction = null)
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            var redisOptions = EngineContext.Current.Configuration.GetRateLimitRedisOptions();
            services
                .AddSilkyHttpCore()
                .AddClientRateLimit(redisOptions)
                .AddIpRateLimit(redisOptions)
                .AddResponseCaching()
                .AddHttpContextAccessor()
                .AddRouting()
                .AddSilkyIdentity<TAuthorizationHandler>(authorizationOptionAction)
                .AddSilkyMiniProfiler()
                .AddSilkyCaching()
                .AddDashboard()
                .AddSwaggerDocuments(swaggerOptionAction);
            services.AddMvc();

            return services;
        }
    }
}