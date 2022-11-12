using System.Threading;
using System.Threading.Tasks;
using Silky.Core.Extensions;

namespace Silky.Rpc.CachingInterceptor
{
    public static class DistributedInterceptCacheExtensions
    {
        public static async Task RemoveAsync(this IDistributedInterceptCache cache, string cacheKey, string cacheName,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            if (!cacheName.IsNullOrEmpty())
            {
                cache.UpdateCacheName(cacheName);
            }

            await cache.RemoveAsync(cacheKey, hideErrors, token);
        }
        
        public static async Task RemoveMatchKeyAsync(this IDistributedInterceptCache cache, string cacheKey, string cacheName,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            if (!cacheName.IsNullOrEmpty())
            {
                cache.UpdateCacheName(cacheName);
            }

            await cache.RemoveMatchKeyAsync(cacheKey, hideErrors, token);
        }
    }
}