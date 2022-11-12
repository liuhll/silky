using System;
using System.Diagnostics.CodeAnalysis;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
{
    public RemoveCachingInterceptAttribute([NotNull] string cacheName, [NotNull] string keyTemplate)
    {
        Check.NotNullOrEmpty(cacheName, nameof(cacheName));
        Check.NotNullOrEmpty(keyTemplate, nameof(keyTemplate));
        CacheName = cacheName;
        KeyTemplate = keyTemplate;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
    }

    public RemoveCachingInterceptAttribute([NotNull] Type cacheType, [NotNull] string keyTemplate)
    {
        Check.NotNull(cacheType, nameof(cacheType));
        Check.NotNullOrEmpty(keyTemplate, nameof(keyTemplate));
        CacheName = cacheType.FullName;
        KeyTemplate = keyTemplate;
        OnlyCurrentUserData = false;
        CachingMethod = CachingMethod.Remove;
    }

    public string CacheName { get; set; }
    public string KeyTemplate { get; }

    public bool IgnoreMultiTenancy { get; set; }
    public CachingMethod CachingMethod { get; }

    public bool OnlyCurrentUserData { get; set; }

    CachingInterceptorDescriptor ICachingInterceptProvider.CachingInterceptorDescriptor { get; set; }
}