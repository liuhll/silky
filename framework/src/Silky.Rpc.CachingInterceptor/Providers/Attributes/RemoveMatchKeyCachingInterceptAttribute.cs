using System;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Rpc.CachingInterceptor.Providers;

namespace Silky.Rpc.CachingInterceptor;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RemoveMatchKeyCachingInterceptAttribute : Attribute, IRemoveMatchKeyCachingInterceptProvider
{
    public RemoveMatchKeyCachingInterceptAttribute( [NotNull] string keyPattern)
    {
        Check.NotNullOrEmpty(keyPattern, nameof(keyPattern));
        KeyTemplete = keyPattern;
        OnlyCurrentUserData = false;
        IgnoreMultiTenancy = false;
        CachingMethod = CachingMethod.Remove;
    }

    public string KeyTemplete { get; }
    public bool OnlyCurrentUserData { get; set; }
    public bool IgnoreMultiTenancy { get; set; }
    public CachingMethod CachingMethod { get; } = CachingMethod.Remove;
}