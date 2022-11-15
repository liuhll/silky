using System;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Extensions;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RemoveMatchKeyCachingInterceptAttribute : Attribute, IRemoveMatchKeyCachingInterceptProvider
{
    public RemoveMatchKeyCachingInterceptAttribute([NotNull] string keyPattern)
    {
        Check.NotNullOrEmpty(keyPattern, nameof(keyPattern));
        KeyTemplate = keyPattern;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
    }

    public RemoveMatchKeyCachingInterceptAttribute([NotNull] Type type, [NotNull] string keyPattern)
    {
        Check.NotNull(type, nameof(type));
        Check.NotNullOrEmpty(keyPattern, nameof(keyPattern));
        KeyTemplate = keyPattern;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
        CacheName = type.GetCacheName();
    }
    
    public RemoveMatchKeyCachingInterceptAttribute([NotNull] string cacheName, [NotNull] string keyPattern)
    {
        Check.NotNull(cacheName, nameof(cacheName));
        Check.NotNullOrEmpty(keyPattern, nameof(keyPattern));
        KeyTemplate = keyPattern;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
        CacheName = cacheName;
    }

    public string KeyTemplate { get; }
    public bool OnlyCurrentUserData { get; set; }
    public bool IgnoreMultiTenancy { get; set; }
    public CachingMethod CachingMethod { get; } = CachingMethod.Remove;

    CachingInterceptorDescriptor ICachingInterceptProvider.CachingInterceptorDescriptor { get; set; }

    public string CacheName { get; set; }
}