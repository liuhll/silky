using System;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Rpc.CachingInterceptor.Providers;

namespace Silky.Rpc.CachingInterceptor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UpdateCachingInterceptAttribute : Attribute, IUpdateCachingInterceptProvider
    {
        public UpdateCachingInterceptAttribute([NotNull] string keyTemplete)
        {
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
            IgnoreMultiTenancy = false;
            CachingMethod = CachingMethod.Remove;
        }

        public string KeyTemplete { get; }

        public bool OnlyCurrentUserData { get; set; }
        public bool IgnoreMultiTenancy { get; set; }
        public CachingMethod CachingMethod { get; }
    }
}