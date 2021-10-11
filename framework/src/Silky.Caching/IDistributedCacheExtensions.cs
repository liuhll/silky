using System.Threading.Tasks;

namespace Silky.Caching
{
    public static class IDistributedCacheExtensions
    {
        public static async Task<bool> ExsitAsync<TCacheItem, TCacheKey>(
            this IDistributedCache<TCacheItem, TCacheKey> cache,
            TCacheKey key) where TCacheItem : class
        {
            return await cache.GetAsync(key) != null;
        }
    }
}