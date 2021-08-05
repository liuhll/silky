using System.Threading.Tasks;
using Castle.Core.Internal;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Silky.Caching;
using Silky.Caching.Configuration;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Transaction.Abstraction;
using StackExchange.Redis;

namespace Silky.Transaction.Schedule
{
    public class ScheduledDistributedLockFactory : IScheduledDistributedLockFactory, ITransientDependency
    {
        public async Task<IDistributedLockProvider> CreateDistributedLockProvider(TransRepositorySupport transRepositorySupport)
        {
            IDistributedLockProvider distributedLockProvider = null;
            switch (transRepositorySupport)
            {
                case TransRepositorySupport.Redis:
                    var cacheOptions = EngineContext.Current.GetOptions<SilkyDistributedCacheOptions>();
                    if (cacheOptions.Redis.Configuration.IsNullOrEmpty())
                    {
                        throw new SilkyException("Failed to acquire redis distributed lock during task scheduling", StatusCode.TransactionError);
                    }
                    var connection = await ConnectionMultiplexer.ConnectAsync(cacheOptions.Redis.Configuration); // uses StackExchange.Redis
                    distributedLockProvider = new RedisDistributedSynchronizationProvider(connection.GetDatabase());
                    break;
                default:
                    throw new SilkyException("Failed to acquire distributed lock during task scheduling", StatusCode.TransactionError);
            }

            return distributedLockProvider;
        }
    }
}