using System;
using System.Diagnostics.CodeAnalysis;
using Lms.Core;

namespace Lms.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
    {
        public RemoveCachingInterceptAttribute([NotNull] params RemoveRemoveCachingKeyInfo[] removeCachingKeys)
        {
            Check.NotNull(removeCachingKeys, nameof(removeCachingKeys));
            RemoveRemoveCachingKeyInfos = removeCachingKeys;
            CachingMethod = CachingMethod.Remove;
        }

        public RemoveRemoveCachingKeyInfo[] RemoveRemoveCachingKeyInfos { get; }

        public CachingMethod CachingMethod { get; }
    }

    public class RemoveRemoveCachingKeyInfo
    {
        public RemoveRemoveCachingKeyInfo(string cacheName, string removeKeyTemplete)
        {
            CacheName = cacheName;
            RemoveKeyTemplete = removeKeyTemplete;
        }

        public string CacheName { get; set; }

        public string RemoveKeyTemplete { get; set; }
    }
}