namespace Silky.Rpc.Runtime.Server;

public class NamedCacheKeyProvider : ICacheKeyProvider
{
    public NamedCacheKeyProvider(string propName)
    {
        CacheKeyType = CacheKeyType.Named;
        PropName = propName;
    }

    public int Index { get; }
    public string PropName { get; set; }
    public CacheKeyType CacheKeyType { get; }
}