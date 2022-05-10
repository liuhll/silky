using System;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class CacheKeyAttribute : Attribute, ICacheKeyProvider
{
    public CacheKeyAttribute(int index)
    {
        Index = index;
    }

    public int Index { get; }
    public string PropName { get; set; }
}