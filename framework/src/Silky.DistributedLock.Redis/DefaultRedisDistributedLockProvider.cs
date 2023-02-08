using Medallion.Threading;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace Silky.DistributedLock.Redis;

public class DefaultRedisDistributedLockProvider : IRedisDistributedLockProvider
{
    public IDistributedLock Create(IDatabase database, string lockName)
    {
        return new RedisDistributedLock(lockName, database);
    }
}