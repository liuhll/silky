using System;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Threading;

namespace Silky.Lock.Extensions
{
    public static class DistributedLockExtensions
    {
        public static async Task ExecForHandle(this IDistributedLock @lock, Func<Task> callback,
            int acquireHandlerTimeout = 300, int sleepTime = 300)
        {
            while (true)
            {
                await using var handler = await @lock.TryAcquireAsync(TimeSpan.FromMilliseconds(acquireHandlerTimeout));
                if (handler != null)
                {
                    await callback();
                    break;
                }

                Thread.Sleep(sleepTime);
            }
        }
    }
}