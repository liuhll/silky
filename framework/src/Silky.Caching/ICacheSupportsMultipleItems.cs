using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Silky.Caching
{
    public interface ICacheSupportsMultipleItems
    {
        byte[][] GetMany(
            IEnumerable<string> keys
        );

        Task<byte[][]> GetManyAsync(
            IEnumerable<string> keys,
            CancellationToken token = default
        );

        void SetMany(
            IEnumerable<KeyValuePair<string, byte[]>> items,
            DistributedCacheEntryOptions options
        );

        Task SetManyAsync(
            IEnumerable<KeyValuePair<string, byte[]>> items,
            DistributedCacheEntryOptions options,
            CancellationToken token = default
        );

        Task RemoveMatchKeyAsync(string key, bool? hideErrors, CancellationToken token);

        Task<IReadOnlyCollection<string>> SearchKeys(string pattern, CancellationToken token = default);
    }
}