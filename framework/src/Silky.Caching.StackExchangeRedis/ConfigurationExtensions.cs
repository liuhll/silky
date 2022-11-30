using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;

namespace Silky.Caching.StackExchangeRedis;

public static class ConfigurationExtensions
{
    public static RedisCacheOptions GetRedisCacheOptions(this IConfiguration configuration)
    {
        var redisEnabled = configuration.GetValue<bool>("DistributedCache:Redis:IsEnabled");

        if (!redisEnabled)
        {
            throw new SilkyException("The redis cache service is unavailable");
        }

        var redisCacheOptions = configuration.GetSection("DistributedCache:Redis").Get<RedisCacheOptions>();
        if (redisCacheOptions == null || redisCacheOptions.Configuration.IsNullOrEmpty())
        {
            throw new SilkyException("You must specify the Configuration of the redis service");
        }

        return redisCacheOptions;
    }
}