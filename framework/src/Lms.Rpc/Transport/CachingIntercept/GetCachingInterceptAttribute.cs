using System;
using JetBrains.Annotations;
using Lms.Core;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GetCachingInterceptAttribute : Attribute, IGetCachingInterceptProvider
    {
        public GetCachingInterceptAttribute([NotNull] string cacheName, [NotNull] string keyTemplete)
        {
            Check.NotNullOrEmpty(cacheName, nameof(cacheName));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheName;
            KeyTemplete = keyTemplete;
            CachingMethod = CachingMethod.Get;
        }

        public string CacheName { get; }

        public string KeyTemplete { get; }

        public CachingMethod CachingMethod { get; }
    }
}