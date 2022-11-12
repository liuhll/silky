using System;

namespace Silky.Rpc.Runtime.Server;

[Obsolete]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class CacheKeyAttribute : Attribute, ICacheKeyProvider
{
    public CacheKeyAttribute(int index)
    {
        Index = index;
        CacheKeyType = CacheKeyType.Attribute;
    }

    public int Index { get; }
    
    public string PropName { get; set; }


    public CacheKeyType CacheKeyType { get; }
}