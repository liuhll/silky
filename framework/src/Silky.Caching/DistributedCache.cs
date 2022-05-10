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
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Caching.Configuration;
using Silky.Core;

namespace Silky.Caching
{
    public class DistributedCache<TCacheItem, TCacheKey> : IDistributedCache<TCacheItem, TCacheKey>
        where TCacheItem : class
    {
        public ILogger<DistributedCache<TCacheItem, TCacheKey>> Logger { get; set; }

        protected string CacheName { get; set; }

        protected bool IgnoreMultiTenancy { get; set; }

        protected IDistributedCache Cache { get; }

        protected ICancellationTokenProvider CancellationTokenProvider { get; }

        protected IDistributedCacheSerializer Serializer { get; }

        protected IDistributedCacheKeyNormalizer KeyNormalizer { get; }

        protected SemaphoreSlim SyncSemaphore { get; }

        protected DistributedCacheEntryOptions DefaultCacheOptions;

        protected SilkyDistributedCacheOptions _distributedCacheOption;

        public DistributedCache(
            IOptionsMonitor<SilkyDistributedCacheOptions> distributedCacheOption,
            IDistributedCache cache,
            ICancellationTokenProvider cancellationTokenProvider,
            IDistributedCacheSerializer serializer,
            IDistributedCacheKeyNormalizer keyNormalizer)
        {
            _distributedCacheOption = distributedCacheOption.CurrentValue;
            distributedCacheOption.OnChange((options, s) => _distributedCacheOption = options);
            Cache = cache;
            CancellationTokenProvider = cancellationTokenProvider;
            Logger = NullLogger<DistributedCache<TCacheItem, TCacheKey>>.Instance;
            Serializer = serializer;
            KeyNormalizer = keyNormalizer;

            SyncSemaphore = new SemaphoreSlim(1, 1);

            SetDefaultOptions();
        }


        public TCacheItem Get(TCacheKey key, bool? hideErrors = null)
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

                throw;
            }

            return ToCacheItem(cachedBytes);
        }

        public async Task<TCacheItem> GetAsync(TCacheKey key, bool? hideErrors = null,
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

                throw;
            }

            return ToCacheItem(cachedBytes);
        }

        public KeyValuePair<TCacheKey, TCacheItem>[] GetMany(IEnumerable<TCacheKey> keys, bool? hideErrors = null)
        {
            var keyArray = keys.ToArray();

            var cacheSupportsMultipleItems = Cache as ICacheSupportsMultipleItems;
            if (cacheSupportsMultipleItems == null)
            {
                return GetManyFallback(
                    keyArray,
                    hideErrors
                );
            }

            var notCachedKeys = new List<TCacheKey>();
            var cachedValues = new List<KeyValuePair<TCacheKey, TCacheItem>>();
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;
            byte[][] cachedBytes;

            var readKeys = notCachedKeys.Any() ? notCachedKeys.ToArray() : keyArray;
            try
            {
                cachedBytes = cacheSupportsMultipleItems.GetMany(readKeys.Select(NormalizeKey));
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    HandleException(ex);
                    return ToCacheItemsWithDefaultValues(keyArray);
                }

                throw;
            }

            return cachedValues.Concat(ToCacheItems(cachedBytes, readKeys)).ToArray();
        }

        public virtual async Task<KeyValuePair<TCacheKey, TCacheItem>[]> GetManyAsync(
            IEnumerable<TCacheKey> keys,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            var keyArray = keys.ToArray();

            var cacheSupportsMultipleItems = Cache as ICacheSupportsMultipleItems;
            if (cacheSupportsMultipleItems == null)
            {
                return await GetManyFallbackAsync(
                    keyArray,
                    hideErrors,
                    token
                );
            }

            var notCachedKeys = new List<TCacheKey>();
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;
            byte[][] cachedBytes;

            var readKeys = notCachedKeys.Any() ? notCachedKeys.ToArray() : keyArray;
            try
            {
                cachedBytes = await cacheSupportsMultipleItems.GetManyAsync(
                    readKeys.Select(NormalizeKey),
                    CancellationTokenProvider.FallbackToProvider(token)
                );
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    await HandleExceptionAsync(ex);
                    return ToCacheItemsWithDefaultValues(keyArray);
                }

                throw;
            }

            return ToCacheItems(cachedBytes, readKeys);
        }


        public TCacheItem GetOrAdd(TCacheKey key, Func<TCacheItem> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null)
        {
            var value = Get(key, hideErrors);
            if (value != null)
            {
                return value;
            }

            using (SyncSemaphore.Lock())
            {
                value = Get(key, hideErrors);
                if (value != null)
                {
                    return value;
                }

                value = factory();

                Set(key, value, optionsFactory?.Invoke(), hideErrors);
            }

            return value;
        }

        public async Task<TCacheItem> GetOrAddAsync(TCacheKey key, Func<Task<TCacheItem>> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null,
            CancellationToken token = default)
        {
            token = CancellationTokenProvider.FallbackToProvider(token);
            var value = await GetAsync(key, hideErrors, token);
            if (value != null)
            {
                return value;
            }

            using (await SyncSemaphore.LockAsync(token))
            {
                value = await GetAsync(key, hideErrors, token);
                if (value != null)
                {
                    return value;
                }

                value = await factory();


                await SetAsync(key, value, optionsFactory?.Invoke(), hideErrors, token);
            }

            return value;
        }

        public void Set(TCacheKey key, TCacheItem value, DistributedCacheEntryOptions options = null,
            bool? hideErrors = null)
        {
            void SetRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    Cache.Set(
                        NormalizeKey(key),
                        Serializer.Serialize(value),
                        options ?? DefaultCacheOptions
                    );
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        HandleException(ex);
                        return;
                    }

                    throw;
                }
            }

            SetRealCache();
        }

        public async Task SetAsync(TCacheKey key, TCacheItem value, DistributedCacheEntryOptions options = null,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            async Task SetRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    await Cache.SetAsync(
                        NormalizeKey(key),
                        Serializer.Serialize(value),
                        options ?? DefaultCacheOptions,
                        CancellationTokenProvider.FallbackToProvider(token)
                    );
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        await HandleExceptionAsync(ex);
                        return;
                    }

                    throw;
                }
            }

            await SetRealCache();
        }

        public void SetMany(IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
            DistributedCacheEntryOptions options = null, bool? hideErrors = null)
        {
            var itemsArray = items.ToArray();

            var cacheSupportsMultipleItems = Cache as ICacheSupportsMultipleItems;
            if (cacheSupportsMultipleItems == null)
            {
                SetManyFallback(
                    itemsArray,
                    options,
                    hideErrors
                );

                return;
            }

            void SetRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    cacheSupportsMultipleItems.SetMany(
                        ToRawCacheItems(itemsArray),
                        options ?? DefaultCacheOptions
                    );
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        HandleException(ex);
                        return;
                    }

                    throw;
                }
            }

            SetRealCache();
        }

        public async Task SetManyAsync(IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
            DistributedCacheEntryOptions options = null, bool? hideErrors = null,
            CancellationToken token = default)
        {
            var itemsArray = items.ToArray();

            var cacheSupportsMultipleItems = Cache as ICacheSupportsMultipleItems;
            if (cacheSupportsMultipleItems == null)
            {
                await SetManyFallbackAsync(
                    itemsArray,
                    options,
                    hideErrors,
                    token
                );

                return;
            }

            async Task SetRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    await cacheSupportsMultipleItems.SetManyAsync(
                        ToRawCacheItems(itemsArray),
                        options ?? DefaultCacheOptions,
                        CancellationTokenProvider.FallbackToProvider(token)
                    );
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        await HandleExceptionAsync(ex);
                        return;
                    }

                    throw;
                }
            }

            await SetRealCache();
        }

        public void Refresh(TCacheKey key, bool? hideErrors = null)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                Cache.Refresh(NormalizeKey(key));
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    HandleException(ex);
                    return;
                }

                throw;
            }
        }

        public async Task RefreshAsync(TCacheKey key, bool? hideErrors = null, CancellationToken token = default)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                await Cache.RefreshAsync(NormalizeKey(key), CancellationTokenProvider.FallbackToProvider(token));
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    await HandleExceptionAsync(ex);
                    return;
                }

                throw;
            }
        }

        public void Remove(TCacheKey key, bool? hideErrors = null)
        {
            void RemoveRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    Cache.Remove(NormalizeKey(key));
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        HandleException(ex);
                        return;
                    }

                    throw;
                }
            }

            RemoveRealCache();
        }

        public async virtual Task RemoveAsync(TCacheKey key, bool? hideErrors = null, CancellationToken token = default)
        {
            async Task RemoveRealCache()
            {
                hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

                try
                {
                    await Cache.RemoveAsync(NormalizeKey(key), CancellationTokenProvider.FallbackToProvider(token));
                }
                catch (Exception ex)
                {
                    if (hideErrors == true)
                    {
                        await HandleExceptionAsync(ex);
                        return;
                    }

                    throw;
                }
            }

            await RemoveRealCache();
        }

        protected virtual string NormalizeKey(TCacheKey key)
        {
            return KeyNormalizer.NormalizeKey(
                new DistributedCacheKeyNormalizeArgs(
                    key.ToString(),
                    CacheName,
                    IgnoreMultiTenancy
                )
            );
        }

        protected virtual void SetDefaultOptions()
        {
            CacheName = CacheNameAttribute.GetCacheName(typeof(TCacheItem));

            //IgnoreMultiTenancy
            //IgnoreMultiTenancy = typeof(TCacheItem).IsDefined(typeof(IgnoreMultiTenancyAttribute), true);

            //Configure default cache entry options
            DefaultCacheOptions = GetDefaultCacheEntryOptions();
        }

        protected DistributedCacheEntryOptions GetDefaultCacheEntryOptions()
        {
            return new DistributedCacheEntryOptions();
        }

        protected virtual void HandleException(Exception ex)
        {
            _ = HandleExceptionAsync(ex);
        }

        protected virtual async Task HandleExceptionAsync(Exception ex)
        {
            Logger.LogException(ex, LogLevel.Warning);

            // using (var scope = ServiceScopeFactory.CreateScope())
            // {
            //     await scope.RpcServices
            //         .GetRequiredService<IExceptionNotifier>()
            //         .NotifyAsync(new ExceptionNotificationContext(ex, LogLevel.Warning));
            // }
        }

        [CanBeNull]
        protected virtual TCacheItem ToCacheItem([CanBeNull] byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            return Serializer.Deserialize<TCacheItem>(bytes);
        }

        protected virtual KeyValuePair<TCacheKey, TCacheItem>[] GetManyFallback(
            TCacheKey[] keys,
            bool? hideErrors = null)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                return keys
                    .Select(key => new KeyValuePair<TCacheKey, TCacheItem>(
                            key,
                            Get(key, false)
                        )
                    ).ToArray();
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    HandleException(ex);
                    return ToCacheItemsWithDefaultValues(keys);
                }

                throw;
            }
        }

        protected virtual KeyValuePair<TCacheKey, TCacheItem>[] ToCacheItems(byte[][] itemBytes, TCacheKey[] itemKeys)
        {
            if (itemBytes.Length != itemKeys.Length)
            {
                throw new SilkyException("count of the item bytes should be same with the count of the given keys");
            }

            var result = new List<KeyValuePair<TCacheKey, TCacheItem>>();

            for (int i = 0; i < itemKeys.Length; i++)
            {
                result.Add(
                    new KeyValuePair<TCacheKey, TCacheItem>(
                        itemKeys[i],
                        ToCacheItem(itemBytes[i])
                    )
                );
            }

            return result.ToArray();
        }

        private static KeyValuePair<TCacheKey, TCacheItem>[] ToCacheItemsWithDefaultValues(TCacheKey[] keys)
        {
            return keys
                .Select(key => new KeyValuePair<TCacheKey, TCacheItem>(key, default))
                .ToArray();
        }

        protected virtual async Task<KeyValuePair<TCacheKey, TCacheItem>[]> GetManyFallbackAsync(
            TCacheKey[] keys,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                var result = new List<KeyValuePair<TCacheKey, TCacheItem>>();

                foreach (var key in keys)
                {
                    result.Add(new KeyValuePair<TCacheKey, TCacheItem>(
                        key,
                        await GetAsync(key, false, token: token))
                    );
                }

                return result.ToArray();
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    await HandleExceptionAsync(ex);
                    return ToCacheItemsWithDefaultValues(keys);
                }

                throw;
            }
        }

        protected virtual void SetManyFallback(
            KeyValuePair<TCacheKey, TCacheItem>[] items,
            DistributedCacheEntryOptions options = null,
            bool? hideErrors = null,
            bool considerUow = false)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                foreach (var item in items)
                {
                    Set(
                        item.Key,
                        item.Value,
                        options,
                        false
                    );
                }
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    HandleException(ex);
                    return;
                }

                throw;
            }
        }

        protected virtual KeyValuePair<string, byte[]>[] ToRawCacheItems(KeyValuePair<TCacheKey, TCacheItem>[] items)
        {
            return items
                .Select(i => new KeyValuePair<string, byte[]>(
                        NormalizeKey(i.Key),
                        Serializer.Serialize(i.Value)
                    )
                ).ToArray();
        }

        protected virtual async Task SetManyFallbackAsync(
            KeyValuePair<TCacheKey, TCacheItem>[] items,
            DistributedCacheEntryOptions options = null,
            bool? hideErrors = null,
            CancellationToken token = default)
        {
            hideErrors = hideErrors ?? _distributedCacheOption.HideErrors;

            try
            {
                foreach (var item in items)
                {
                    await SetAsync(
                        item.Key,
                        item.Value,
                        options,
                        false,
                        token: token
                    );
                }
            }
            catch (Exception ex)
            {
                if (hideErrors == true)
                {
                    await HandleExceptionAsync(ex);
                    return;
                }

                throw;
            }
        }
    }

    public class DistributedCache<TCacheItem> : DistributedCache<TCacheItem, string>, IDistributedCache<TCacheItem>
        where TCacheItem : class
    {
        public DistributedCache(
            IOptionsMonitor<SilkyDistributedCacheOptions> distributedCacheOption,
            IDistributedCache cache,
            ICancellationTokenProvider cancellationTokenProvider,
            IDistributedCacheSerializer serializer,
            IDistributedCacheKeyNormalizer keyNormalizer) : base(
            distributedCacheOption: distributedCacheOption,
            cache: cache,
            cancellationTokenProvider: cancellationTokenProvider,
            serializer: serializer,
            keyNormalizer: keyNormalizer)
        {
        }

        public async Task<IReadOnlyCollection<string>> SearchKeys(string keyPattern)
        {
            var distributedInterceptCache = (Cache as ICacheSupportsMultipleItems);
            if (distributedInterceptCache == null)
            {
                var cacheKeys = GetCacheKeys();
                return cacheKeys.Where(k => Regex.IsMatch(k, keyPattern)).ToImmutableArray();
            }

            return await distributedInterceptCache.SearchKeys(keyPattern);
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
    }
}