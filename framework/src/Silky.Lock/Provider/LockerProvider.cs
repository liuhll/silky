using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Silky.Lock.Configuration;
using StackExchange.Redis;

namespace Silky.Lock.Provider
{
    public class LockerProvider : ILockerProvider, ITransientDependency
    {

        private readonly RedLockFactory _redLockFactory;
        private readonly LockOptions _lockOptions;
        public LockerProvider(IOptions<LockOptions> lockOptions) 
        { 
            _lockOptions = lockOptions.Value;
            _redLockFactory = RedLockFactory.Create(GetRedLockMultiplexers());
           
        }

        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry,TimeSpan wait, TimeSpan retry)
        {
            return _redLockFactory.CreateLockAsync(resource, expiry, wait, retry);

        }

        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry) 
        {
            return CreateLockAsync(resource, expiry, _lockOptions.WaitTimeSpan, _lockOptions.RetryTimeSpan);

        }

        public Task<IRedLock> CreateLockAsync(string resource)
        {
            return CreateLockAsync(resource, _lockOptions.DefaultExpiryTimeSpan, _lockOptions.WaitTimeSpan, _lockOptions.RetryTimeSpan);
        }

        void IDisposable.Dispose()
        {
            _redLockFactory?.Dispose();
        }

        private IList<RedLockMultiplexer> GetRedLockMultiplexers() 
        {
            if (_lockOptions == null) 
            {
                throw new ArgumentNullException("没有设置分布式锁服务");
            }

            if (_lockOptions.LockRedisConnection.IsNullOrEmpty())
            {
                throw new SilkyException("未配置分布式锁服务地址");
            }
            var multiplexers = new List<RedLockMultiplexer>();
            var existingConnectionMultiplexer = ConnectionMultiplexer.Connect(_lockOptions.LockRedisConnection);
            multiplexers.Add(existingConnectionMultiplexer);
            return multiplexers;
        }
    }
}