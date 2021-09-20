using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Silky.Core;
using Silky.Http.RateLimit.Configuration;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RateLimitServiceCollectionExtensions
    {
        public static IServiceCollection AddClientRateLimit(this IServiceCollection services,
            ConfigurationOptions redisOptions = null)
        {
            services.AddMemoryCache();

            services.Configure<ClientRateLimitOptions>(
                EngineContext.Current.Configuration.GetSection(SilkyRateLimitOptions.ClientRateLimit));

            services.Configure<ClientRateLimitPolicies>(
                EngineContext.Current.Configuration.GetSection(SilkyRateLimitOptions.ClientRateLimitPolicies));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            if (redisOptions != null)
            {
                services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisOptions));
                services.AddRedisRateLimiting();
            }
            else
            {
                services.AddInMemoryRateLimiting();
            }

            return services;
        }

        public static IServiceCollection AddIpRateLimit(this IServiceCollection services,
            ConfigurationOptions redisOptions = null)
        {
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(
                EngineContext.Current.Configuration.GetSection(SilkyRateLimitOptions.IpRateLimit));

            services.Configure<IpRateLimitPolicies>(
                EngineContext.Current.Configuration.GetSection(SilkyRateLimitOptions.IpRateLimitPolicies));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            if (redisOptions != null)
            {
                services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisOptions));
                services.AddRedisRateLimiting();
            }
            else
            {
                services.AddInMemoryRateLimiting();
            }

            return services;
        }
    }
}