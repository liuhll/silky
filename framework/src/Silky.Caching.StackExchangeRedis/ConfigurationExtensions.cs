using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Silky.Caching.Configuration;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using StackExchange.Redis;

namespace Silky.Caching.StackExchangeRedis;

public static class ConfigurationExtensions
{
    public static RedisCacheOptions GetRedisCacheOptions(this IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection("DistributedCache:Redis").Get<RedisOptions>();

        if (!redisOptions.IsEnabled)
        {
            throw new SilkyException("The redis cache service is unavailable");
        }

        if (redisOptions.Configuration.IsNullOrEmpty())
        {
            throw new SilkyException("You must specify the Configuration of the redis service");
        }

        var redisCacheOptions = new RedisCacheOptions()
        {
            Configuration = redisOptions.Configuration,
            ConfigurationOptions = ConfigurationOptions.Parse(redisOptions.Configuration),
        };
        return redisCacheOptions;
    }
}