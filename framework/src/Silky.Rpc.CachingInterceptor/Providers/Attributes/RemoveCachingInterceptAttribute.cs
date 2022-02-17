using System;
using System.Diagnostics.CodeAnalysis;
using Silky.Core;
using Silky.Rpc.CachingInterceptor.Providers;

namespace Silky.Rpc.CachingInterceptor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
    {
        public RemoveCachingInterceptAttribute([NotNull] string cacheName, [NotNull] string keyTemplete)
        {
            Check.NotNullOrEmpty(cacheName, nameof(cacheName));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheName;
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
            IgnoreMultiTenancy = false;
            CachingMethod = CachingMethod.Remove;
        }
        
        public RemoveCachingInterceptAttribute([NotNull] Type cacheType, [NotNull] string keyTemplete)
        {
            Check.NotNull(cacheType, nameof(cacheType));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheType.FullName;
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
            CachingMethod = CachingMethod.Remove;
        }

        public string CacheName { get; }
        public string KeyTemplete { get; }

        public bool IgnoreMultiTenancy { get; set; }
        public CachingMethod CachingMethod { get; }

        public bool OnlyCurrentUserData { get; set; }
    }
}