using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static ConfigurationOptions GetRateLimitRedisOptions(this IConfiguration configuration)
        {
            var rateLimitingRedisConfigurationSection = configuration.GetSection("RateLimiting:RedisConfiguration");
            if (rateLimitingRedisConfigurationSection.Exists())
            {
                var redisOptions = ConfigurationOptions.Parse(configuration["RateLimiting:RedisConfiguration"]);
                return redisOptions;
            }

            return null;
        }
    }
}