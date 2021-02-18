namespace Lms.Caching
{
    public static class IDistributedCacheExtensions
    {
        public static void UpdateCacheName<TCacheItem>(this IDistributedCache<TCacheItem, string> distributedCache,
            string cacheName)
            where TCacheItem : class
        {
            var lmsDistributedCache =
                distributedCache as DistributedCache<TCacheItem, string>;
            if (lmsDistributedCache != null)
            {
                lmsDistributedCache.CacheName = cacheName;
            }
        }
    }
}