using System.Threading;
using System.Threading.Tasks;

namespace Silky.Rpc.CachingInterceptor
{
    public static class DistributedInterceptCacheExtensions
    {
        public static async Task RemoveAsync(this IDistributedInterceptCache cache, string cacheKey, string cacheName,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            cache.UpdateCacheName(cacheName);
            await cache.RemoveAsync(cacheKey, hideErrors, token);
        }
    }
}