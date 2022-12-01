using Microsoft.Extensions.Caching.StackExchangeRedis;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Repository.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisTransactionRepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisTransactionRepository(this IServiceCollection services,
            RedisCacheOptions redisCacheOption)

        {
            Check.NotNull(redisCacheOption, nameof(redisCacheOption));
            if (!services.IsAddedImplementationType(typeof(SilkyRedisCache)))
            {
                services.AddRedisCaching(redisCacheOption);
            }
            services.AddSingleton(typeof(ITransRepository), typeof(RedisTransRepository));
            services.AddRedisLock(redisCacheOption);
            return services;
        }
    }
}