using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Silky.Caching;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.CachingInterceptor
{
    public interface IDistributedInterceptCache : IDistributedCache<object, string>, IScopedDependency
    {
        void UpdateCacheName(string cacheName);
        
        void SetIgnoreMultiTenancy(bool ignoreMultiTenancy);

        Task<object> GetOrAddAsync(
            [NotNull] string key,
            [NotNull] Type type,
            Func<Task<object>> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        object Get(string key, Type type, bool? hideErrors = null);

        Task<object> GetAsync(string key, Type type, bool? hideErrors = null,
            CancellationToken token = default);
    }
}