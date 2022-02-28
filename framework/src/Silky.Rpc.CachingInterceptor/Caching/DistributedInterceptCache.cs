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
using Microsoft.Extensions.Options;
using Silky.Caching;
using Silky.Caching.Configuration;
using Silky.Core.Logging;
using Silky.Core.Threading;

namespace Silky.Rpc.CachingInterceptor
{
    public class DistributedInterceptCache : DistributedCache<object, string>, IDistributedInterceptCache
    {
        public DistributedInterceptCache(IOptionsMonitor<SilkyDistributedCacheOptions> distributedCacheOption,
            IDistributedCache cache,
            ICancellationTokenProvider cancellationTokenProvider,
            IDistributedCacheSerializer serializer,
            IDistributedCacheKeyNormalizer keyNormalizer)
            : base(distributedCacheOption, 
                cache,
                cancellationTokenProvider,
                serializer,
                keyNormalizer)
        {
        }

        public void UpdateCacheName(string cacheName)
        {
            CacheName = cacheName;
        }

        public void SetIgnoreMultiTenancy(bool ignoreMultiTenancy)
        {
            IgnoreMultiTenancy = ignoreMultiTenancy;
        }

        public async Task RemoveMatchKeyAsync(string keyPattern, bool? hideErrors = null,
            CancellationToken token = default)
        {
            using (await SyncSemaphore.LockAsync(token))
            {
                if (Cache is not ICacheSupportsMultipleItems cacheSupportsMultipleItems)
                {
                    var matchKeys = SearchKeys(keyPattern);
                    foreach (var matchKey in matchKeys)
                    {
                        await RemoveAsync(matchKey, hideErrors, token);
                    }
                }
                else
                {
                    await cacheSupportsMultipleItems.RemoveMatchKeyAsync(keyPattern, hideErrors, token);
                }
            }
        }

        public async Task<object> GetOrAddAsync(string key, Type type, Func<Task<object>> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null,
            CancellationToken token = default)
        {
            token = CancellationTokenProvider.FallbackToProvider(token);
            var value = await GetAsync(key, type, hideErrors, token);
            if (value != null)
            {
                return value;
            }

            using (await SyncSemaphore.LockAsync(token))
            {
                value = await GetAsync(key, type, hideErrors, token);
                if (value != null)
                {
                    return value;
                }

                value = await factory();

                await SetAsync(key, value, optionsFactory?.Invoke(), hideErrors, token);
            }

            return value;
        }

        public object Get(string key, Type type, bool? hideErrors = null)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;
            byte[] cachedBytes;

            try
            {
                cachedBytes = Cache.Get(NormalizeKey(key));
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    HandleException(ex);
                    return null;
                }

                Logger.LogException(ex);
                throw;
            }

            return ToCacheItem(cachedBytes, type);
        }

        public async Task<object> GetAsync(string key, Type type, bool? hideErrors = null,
            CancellationToken token = default)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;
            byte[] cachedBytes;

            try
            {
                cachedBytes = await Cache.GetAsync(
                    NormalizeKey(key),
                    CancellationTokenProvider.FallbackToProvider(token)
                );
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    await HandleExceptionAsync(ex);
                    return null;
                }

                Logger.LogException(ex);
                throw;
            }

            return ToCacheItem(cachedBytes, type);
        }

        public override async Task RemoveAsync(string key, bool? hideErrors = null, CancellationToken token = default)
        {
            if (key.Contains("*") || key.Contains("^") || key.Contains("$") || key.Contains("?"))
            {
                var distributedInterceptCache = (Cache as ICacheSupportsMultipleItems);
                if (distributedInterceptCache == null)
                {
                    var matchKeys = SearchKeys(key);
                    foreach (var matchKey in matchKeys)
                    {
                        await base.RemoveAsync(matchKey, hideErrors, token);
                    }
                }
                else
                {
                    await distributedInterceptCache.RemoveMatchKeyAsync(NormalizeKey(key), hideErrors, token);
                }
            }
            else
            {
                await base.RemoveAsync(key, hideErrors, token);
            }
        }

        protected virtual IReadOnlyCollection<string> SearchKeys(string key)
        {
            var cacheKeys = GetCacheKeys();
            return cacheKeys.Where(k => Regex.IsMatch(k, key)).ToImmutableArray();
        }

        private List<string> GetCacheKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var memCache = Cache.GetType().GetField("_memCache", flags).GetValue(Cache);
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

        [CanBeNull]
        protected virtual object ToCacheItem([CanBeNull] byte[] bytes, Type type)
        {
            if (bytes == null)
            {
                return null;
            }

            return Serializer.Deserialize(bytes, type);
        }
    }
}