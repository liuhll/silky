using Silky.Core.Extensions;

namespace Silky.Rpc.Runtime.Server;

public static class CachingInterceptorDescriptorExtensions
{
    public static CacheKeyType GetCacheKeyType(this CachingInterceptorDescriptor cachingInterceptorDescriptor)
    {
        if (cachingInterceptorDescriptor.KeyTemplate.IsMatch(CacheKeyConstants.CacheKeyParameterRegex))
        {
            return CacheKeyType.Named;
        }

        return CacheKeyType.Attribute;
    }
}