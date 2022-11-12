namespace Silky.Rpc.Runtime.Server;

public class NamedCacheKeyProvider : ICacheKeyProvider
{
    
    public NamedCacheKeyProvider(string propName, int index)
    {
        PropName = propName;
        Index = index;
        CacheKeyType = CacheKeyType.Named;
    }

    public int Index { get; }
    
    public string PropName { get; set; }
    public CacheKeyType CacheKeyType { get; }
}