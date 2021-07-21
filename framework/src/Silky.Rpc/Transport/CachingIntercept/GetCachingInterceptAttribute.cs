using System;
using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Transport.CachingIntercept
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
        }
        
        public string KeyTemplete { get; }
        
        public bool OnlyCurrentUserData { get; set; }

        public CachingMethod CachingMethod { get; }
    }
}