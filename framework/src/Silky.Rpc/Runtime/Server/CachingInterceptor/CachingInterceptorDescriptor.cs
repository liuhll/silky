using System.Collections.Generic;

namespace Silky.Rpc.Runtime.Server;

public class CachingInterceptorDescriptor
{
    public CachingInterceptorDescriptor()
    {
        CacheKeyProviderDescriptors = new List<CacheKeyProviderDescriptor>();
        IsRemoveMatchKeyProvider = false;
    }

    public string KeyTemplate { get; set;}

    public bool OnlyCurrentUserData { get; set; }

    public bool IgnoreMultiTenancy { get; set; }

    public CachingMethod CachingMethod { get; set; }

    public string CacheName { get; set; }

    public bool IsRemoveMatchKeyProvider { get; set; }

    public ICollection<CacheKeyProviderDescriptor> CacheKeyProviderDescriptors { get; set; }
    
    public bool? IgnoreWhenCacheKeyNull { get; set; }
}