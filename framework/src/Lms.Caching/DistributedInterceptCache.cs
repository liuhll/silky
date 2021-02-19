using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Caching.Configuration;
using Lms.Core.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Lms.Caching
{
    public class DistributedInterceptCache : DistributedCache<object, string>, IDistributedInterceptCache
    {
        public DistributedInterceptCache(IOptions<LmsDistributedCacheOptions> distributedCacheOption,
            IDistributedCache cache,
            ICancellationTokenProvider cancellationTokenProvider,
            IDistributedCacheSerializer serializer,
            IDistributedCacheKeyNormalizer keyNormalizer)
            : base(distributedCacheOption, cache, cancellationTokenProvider, serializer, keyNormalizer)
        {
        }

        public void UpdateCacheName(string cacheName)
        {
            CacheName = cacheName;
        }

        public async Task<object> GetOrAddAsync(string key, Type type, Func<Task<object>> factory,
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

                throw;
            }

            return ToCacheItem(cachedBytes, type);
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