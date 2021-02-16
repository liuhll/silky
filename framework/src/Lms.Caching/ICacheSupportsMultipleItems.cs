using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Lms.Caching
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
    }
}