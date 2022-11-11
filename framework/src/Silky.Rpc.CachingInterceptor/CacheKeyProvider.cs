using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor;

public class CacheKeyProvider : ICacheKeyProvider
{
    public int Index { get; set; }
    public string PropName { get; set; }

    public CacheKeyType CacheKeyType { get; set; }

    public string Value { get; set; }
}