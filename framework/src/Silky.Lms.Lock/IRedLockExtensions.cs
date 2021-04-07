using System;
using System.Threading;
using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Microsoft.Extensions.Options;
using RedLockNet;
using Silky.Lms.Lock.Configuration;

namespace Silky.Lms.Lock
{
    public static class IRedLockExtensions
    {
        public static async Task Lock(this IRedLock redLock, Func<Task> callback)
        {
            var lockOptions = EngineContext.Current.Resolve<IOptions<LockOptions>>().Value;
            var lockerTime = DateTime.Now;
            while (true) 
            {
                if (redLock.IsAcquired) 
                {
                    await callback();
                    break;
                }
                Thread.Sleep(lockOptions.RetryTimeSpan);
                if (DateTime.Now - lockerTime > lockOptions.WaitTimeSpan) 
                {
                    throw new LmsException($"获取分布式锁资源{redLock.Resource}超时");
                }
            }
        }

        public static async Task<T> Lock<T>(this IRedLock redLock, Func<Task<T>> callback)
        {
            var lockerTime = DateTime.Now;
            var lockOptions = EngineContext.Current.Resolve<IOptions<LockOptions>>().Value;
            while (true)
            {
                if (redLock.IsAcquired)
                {
                    return await callback();
                }
                Thread.Sleep(lockOptions.RetryTimeSpan);
                if (DateTime.Now - lockerTime >lockOptions.WaitTimeSpan)
                {
                    throw new LmsException($"获取分布式锁资源{redLock.Resource}超时");
                }
            }
        }
    }
}