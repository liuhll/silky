using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Server;

public class CachingInterceptorDescriptor
{
    public CachingInterceptorDescriptor()
    {
        CacheKeyProviders = new List<CacheKeyProviderDescriptor>();
        IsRemoveMatchKeyProvider = false;
    }

    public string KeyTemplete { get; set;}

    public bool OnlyCurrentUserData { get; set; }

    public bool IgnoreMultiTenancy { get; set; }

    public CachingMethod CachingMethod { get; set; }

    public string CacheName { get; set; }

    public bool IsRemoveMatchKeyProvider { get; set; }

    public ICollection<CacheKeyProviderDescriptor> CacheKeyProviders { get; set; }
}