using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RemoveMatchKeyCachingInterceptAttribute : Attribute, IRemoveMatchKeyCachingInterceptProvider
{
    public RemoveMatchKeyCachingInterceptAttribute( [NotNull] string keyPattern)
    {
        Check.NotNullOrEmpty(keyPattern, nameof(keyPattern));
        KeyTemplate = keyPattern;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
    }

    public string KeyTemplate { get; }
    public bool OnlyCurrentUserData { get; set; }
    public bool IgnoreMultiTenancy { get; set; }
    public CachingMethod CachingMethod { get; } = CachingMethod.Remove;
    
    CachingInterceptorDescriptor ICachingInterceptProvider.CachingInterceptorDescriptor { get; set; }
    
    public string CacheName { get; set; }
}