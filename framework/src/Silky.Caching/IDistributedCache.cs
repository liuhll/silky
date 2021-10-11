using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;

namespace Silky.Caching
{
    public interface IDistributedCache<TCacheItem, TCacheKey>
        where TCacheItem : class
    {
        TCacheItem Get(
            TCacheKey key,
            bool? hideErrors = null
        );

        Task<TCacheItem> GetAsync(
            [NotNull] TCacheKey key,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        KeyValuePair<TCacheKey, TCacheItem>[] GetMany(
            IEnumerable<TCacheKey> keys,
            bool? hideErrors = null
        );

        Task<KeyValuePair<TCacheKey, TCacheItem>[]> GetManyAsync(
            IEnumerable<TCacheKey> keys,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        TCacheItem GetOrAdd(
            TCacheKey key,
            Func<TCacheItem> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null,
            bool? hideErrors = null
        );

        Task<TCacheItem> GetOrAddAsync(
            [NotNull] TCacheKey key,
            Func<Task<TCacheItem>> factory,
            Func<DistributedCacheEntryOptions> optionsFactory = null,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        void Set(
            TCacheKey key,
            TCacheItem value,
            DistributedCacheEntryOptions options = null,
            bool? hideErrors = null
        );

        Task SetAsync(
            [NotNull] TCacheKey key,
            [NotNull] TCacheItem value,
            [CanBeNull] DistributedCacheEntryOptions options = null,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        void SetMany(
            IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
            DistributedCacheEntryOptions options = null,
            bool? hideErrors = null
        );

        Task SetManyAsync(
            IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
            DistributedCacheEntryOptions options = null,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        void Refresh(
            TCacheKey key,
            bool? hideErrors = null
        );

        Task RefreshAsync(
            TCacheKey key,
            bool? hideErrors = null,
            CancellationToken token = default
        );

        void Remove(
            TCacheKey key,
            bool? hideErrors = null
        );

        Task RemoveAsync(
            TCacheKey key,
            bool? hideErrors = null,
            CancellationToken token = default
        );
    }

    public interface IDistributedCache<TCacheItem> : IDistributedCache<TCacheItem, string>
        where TCacheItem : class
    {
        Task<IReadOnlyCollection<string>> SearchKeys(string keyPattern);
    }
}