using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class GetCachingInterceptAttribute : Attribute, IGetCachingInterceptProvider
{
    public GetCachingInterceptAttribute([NotNull] string keyTemplate)
    {
        Check.NotNullOrEmpty(keyTemplate, nameof(keyTemplate));
        KeyTemplate = keyTemplate;
        CachingMethod = CachingMethod.Get;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
    }

    public string KeyTemplate { get; }

    public bool OnlyCurrentUserData { get; set; }
    public bool IgnoreMultiTenancy { get; set; }

    public CachingMethod CachingMethod { get; }

    CachingInterceptorDescriptor ICachingInterceptProvider.CachingInterceptorDescriptor { get; set; }
    
}