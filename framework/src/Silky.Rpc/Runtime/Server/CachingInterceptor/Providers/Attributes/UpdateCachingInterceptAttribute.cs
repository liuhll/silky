using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class UpdateCachingInterceptAttribute : Attribute, IUpdateCachingInterceptProvider
{
    public UpdateCachingInterceptAttribute([NotNull] string keyTemplate)
    {
        Check.NotNullOrEmpty(keyTemplate, nameof(keyTemplate));
        KeyTemplate = keyTemplate;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        IgnoreWhenCacheKeyNull = true;
        CachingMethod = CachingMethod.Update;
    }

    public string KeyTemplate { get; }

    public bool OnlyCurrentUserData { get; set; }
    public bool IgnoreMultiTenancy { get; set; }
    public CachingMethod CachingMethod { get; }

    CachingInterceptorDescriptor ICachingInterceptProvider.CachingInterceptorDescriptor { get; set; }
    
    public bool IgnoreWhenCacheKeyNull { get; set; }
}