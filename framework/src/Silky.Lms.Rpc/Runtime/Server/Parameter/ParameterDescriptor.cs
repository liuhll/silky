using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Rpc.Transport.CachingIntercept;

namespace Silky.Lms.Rpc.Runtime.Server.Parameter
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor(ParameterFrom @from, ParameterInfo parameterInfo, string name = null)
        {
            From = @from;
            ParameterInfo = parameterInfo;
            Name = !name.IsNullOrEmpty() ? name : parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            IsHashKey = DecideIsHashKey();
            CacheKeys = CreateCacheKeys();
        }

        private IReadOnlyCollection<ICacheKeyProvider> CreateCacheKeys()
        {
            var cacheKeys = new List<ICacheKeyProvider>();
            var parameterInfoCacheKeyProvider =
                ParameterInfo.GetCustomAttributes().OfType<ICacheKeyProvider>().FirstOrDefault();
            if (IsSample && parameterInfoCacheKeyProvider != null)
            {
                parameterInfoCacheKeyProvider.PropName = Name;

                cacheKeys.Add(parameterInfoCacheKeyProvider);
            }
            else if (!IsSample && parameterInfoCacheKeyProvider != null)
            {
                throw new LmsException("复杂参数类型不允许使用CacheKeyAttribute");
            }
            else
            {
                var propCacheKeyProviderInfos =
                    Type.GetProperties().Select(p => new
                        {
                            CacheKeyProvider = p.GetCustomAttributes().OfType<ICacheKeyProvider>().FirstOrDefault(),
                            PropertyInfo = p
                        })
                        .Where(p => p.CacheKeyProvider != null);
                foreach (var propCacheKeyProviderInfo in propCacheKeyProviderInfos)
                {
                    var propInfoCacheKeyProvider = propCacheKeyProviderInfo.CacheKeyProvider;
                    propInfoCacheKeyProvider.PropName = propCacheKeyProviderInfo.PropertyInfo.Name;
                    cacheKeys.Add(propInfoCacheKeyProvider);
                }
            }

            return cacheKeys;
        }

        private bool DecideIsHashKey()
        {
            var hashKeyProvider = ParameterInfo.GetCustomAttributes().OfType<IHashKeyProvider>();
            if (hashKeyProvider.Any())
            {
                if (IsSample)
                {
                    return true;
                }
            }

            var propsHashKeyProvider = Type.GetProperties()
                .SelectMany(p => p.GetCustomAttributes().OfType<IHashKeyProvider>());
            if (propsHashKeyProvider.Count() > 1)
            {
                throw new LmsException("不允许指定多个HashKey");
            }

            return false;
        }

        public ParameterFrom From { get; }

        public Type Type { get; }

        public bool IsHashKey { get; }

        public string Name { get; }

        public bool IsSample => Type.IsSample();

        public ParameterInfo ParameterInfo { get; }

        public IReadOnlyCollection<ICacheKeyProvider> CacheKeys { get; }
    }
}