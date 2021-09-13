using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Caching;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisCachingServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCaching(this IServiceCollection services, string redisConfiguration)
        {
            if (redisConfiguration.IsNullOrEmpty())
            {
                throw new SilkyException("You must specify the Configuration of the redis service");
            }

            if (!services.IsAdded(typeof(IDistributedCache<>)))
            {
                services.AddSilkyCaching();
            }

            services.AddStackExchangeRedisCache(options => { options.Configuration = redisConfiguration; });
            services.Replace(ServiceDescriptor.Singleton<IDistributedCache, SilkyRedisCache>());
            return services;
        }

        public static IServiceCollection AddRedisCaching(this IServiceCollection services,
            Action<RedisCacheOptions> setupAction)
        {
            if (!services.IsAdded(typeof(IDistributedCache<>)))
            {
                services.AddSilkyCaching();
            }

            services.AddStackExchangeRedisCache(setupAction);
            services.Replace(ServiceDescriptor.Singleton<IDistributedCache, SilkyRedisCache>());
            return services;
        }

        public static IServiceCollection AddRedisCaching(this IServiceCollection services,
            RedisCacheOptions redisCacheOptions)
        {
            Check.NotNull(redisCacheOptions, nameof(redisCacheOptions));
            if (!services.IsAdded(typeof(IDistributedCache<>)))
            {
                services.AddSilkyCaching();
            }

            services.AddStackExchangeRedisCache(options =>
            {
                if (options == null) throw new ArgumentNullException(nameof(options));
                options.Configuration = redisCacheOptions.Configuration;
                options.ConfigurationOptions = redisCacheOptions.ConfigurationOptions;
                options.InstanceName = redisCacheOptions.InstanceName;
            });
            services.Replace(ServiceDescriptor.Singleton<IDistributedCache, SilkyRedisCache>());
            return services;
        }
    }
}