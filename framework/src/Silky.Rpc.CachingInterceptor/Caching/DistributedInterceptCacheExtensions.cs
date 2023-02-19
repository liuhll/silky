using System.Threading;
using System.Threading.Tasks;

namespace Silky.Rpc.CachingInterceptor
{
    public static class DistributedInterceptCacheExtensions
    {
        public static async Task RemoveForInterceptAsync(this IDistributedInterceptCache cache, string cacheKey, string cacheName,
            bool isMatchKey,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            cache.UpdateCacheName(cacheName);
            if (isMatchKey)
            {
                
                await cache.RemoveMatchKeyAsync(cacheKey, hideErrors, token);
            }
            else
            {
                await cache.RemoveAsync(cacheKey, hideErrors, token);
            }
        }
    }
}