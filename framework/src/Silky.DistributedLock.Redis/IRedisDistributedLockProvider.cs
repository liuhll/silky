using Medallion.Threading;
using StackExchange.Redis;

namespace Silky.DistributedLock.Redis;

public interface IRedisDistributedLockProvider
{
    IDistributedLock Create(IDatabase database, string lockName);

}