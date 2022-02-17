using System;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Rpc.CachingInterceptor.Providers;

namespace Silky.Rpc.CachingInterceptor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GetCachingInterceptAttribute : Attribute, IGetCachingInterceptProvider
    {
        public GetCachingInterceptAttribute([NotNull] string keyTemplete)
        {
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            KeyTemplete = keyTemplete;
            CachingMethod = CachingMethod.Get;
            OnlyCurrentUserData = false;
            IgnoreMultiTenancy = false;
        }

        public string KeyTemplete { get; }

        public bool OnlyCurrentUserData { get; set; }
        public bool IgnoreMultiTenancy { get; set; }

        public CachingMethod CachingMethod { get; }
    }
}