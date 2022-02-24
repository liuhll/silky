using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Silky.Core;

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

        public static Task RemoveAsync([NotNull] this IDistributedCache distributedCache, [NotNull] string cacheName,
            [NotNull] string key)
        {
            return distributedCache.RemoveAsync(NormalizeKey(key, cacheName));
        }

        public static Task RemoveAsync([NotNull] this IDistributedCache distributedCache, [NotNull] Type cacheType,
            [NotNull] string key)
        {
            return distributedCache.RemoveAsync(NormalizeKey(key, cacheType.FullName));
        }

        public static async Task RemoveMatchKeyAsync([NotNull] this IDistributedCache distributedCache,
            [NotNull] string cacheName,
            [NotNull] string keyPattern, bool? hideErrors = null, CancellationToken token = default)
        {
            var cacheSupportsMultipleItems = (distributedCache as ICacheSupportsMultipleItems);
            var key = NormalizeKey(keyPattern, cacheName);
            if (cacheSupportsMultipleItems == null)
            {
                var matchKeys = SearchKeys(key, distributedCache);
                foreach (var matchKey in matchKeys)
                {
                    await distributedCache.RemoveAsync(matchKey, token);
                }
            }
            else
            {
                await cacheSupportsMultipleItems.RemoveMatchKeyAsync(key, hideErrors, token);
            }
        }

        public static async Task RemoveMatchKeyAsync([NotNull] this IDistributedCache distributedCache,
            [NotNull] string keyPattern, bool? hideErrors = null, CancellationToken token = default)
        {
            var cacheSupportsMultipleItems = (distributedCache as ICacheSupportsMultipleItems);
            if (cacheSupportsMultipleItems == null)
            {
                var matchKeys = SearchKeys(keyPattern, distributedCache);
                foreach (var matchKey in matchKeys)
                {
                    await distributedCache.RemoveAsync(matchKey, token);
                }
            }
            else
            {
                await cacheSupportsMultipleItems.RemoveMatchKeyAsync(keyPattern, hideErrors, token);
            }
        }

        public static Task RemoveMatchKeyAsync([NotNull] this IDistributedCache distributedCache,
            [NotNull] Type cacheType,
            [NotNull] string keyPattern, bool? hideErrors = null, CancellationToken token = default)
        {
            return distributedCache.RemoveMatchKeyAsync(cacheType.FullName, keyPattern, hideErrors, token);
        }

        private static IReadOnlyCollection<string> SearchKeys(string key, IDistributedCache distributedCache)
        {
            var cacheKeys = GetCacheKeys(distributedCache);
            return cacheKeys.Where(k => Regex.IsMatch(k, key)).ToImmutableArray();
        }

        private static List<string> GetCacheKeys(IDistributedCache distributedCache)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var memCache = distributedCache.GetType().GetField("_memCache", flags).GetValue(distributedCache);
            Debug.Assert(memCache != null);
            var entries = memCache.GetType().GetField("_entries", flags).GetValue(memCache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null) return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }

            return keys;
        }

        private static string NormalizeKey(string key, string cacheName, bool ignoreMultiTenancy = false)
        {
            var keyNormalizer = EngineContext.Current.Resolve<IDistributedCacheKeyNormalizer>();
            return keyNormalizer.NormalizeKey(
                new DistributedCacheKeyNormalizeArgs(
                    key,
                    cacheName,
                    ignoreMultiTenancy
                )
            );
        }
    }
}