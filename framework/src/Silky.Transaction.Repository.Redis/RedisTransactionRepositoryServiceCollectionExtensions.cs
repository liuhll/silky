using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.DependencyInjection;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Repository.Redis;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisTransactionRepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisTransactionRepository(this IServiceCollection services,
            RedisCacheOptions redisCacheOption)

        {
            if (!services.IsAddedImplementationType(typeof(SilkyRedisCache)))
            {
                services.AddRedisCaching(redisCacheOption);
            }

            services.AddSingleton(typeof(ITransRepository), typeof(RedisTransRepository));
            var connection = ConnectionMultiplexer.Connect(redisCacheOption.Configuration);
            var distributedLockProvider = new RedisDistributedSynchronizationProvider(connection.GetDatabase(15));
            services.AddSingleton<IDistributedLockProvider>(distributedLockProvider);
            return services;
        }
    }
}