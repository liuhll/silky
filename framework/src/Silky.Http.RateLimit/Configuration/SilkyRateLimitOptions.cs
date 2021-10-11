using StackExchange.Redis;

namespace Silky.Http.RateLimit.Configuration
{
    public class SilkyRateLimitOptions
    {
        internal const string ClientRateLimit = "RateLimiting:Client";

        internal const string ClientRateLimitPolicies = "RateLimiting:Client:Policies";

        internal const string IpRateLimit = "RateLimiting:Ip";

        internal const string IpRateLimitPolicies = "RateLimiting:Ip:Policies";

        public ConfigurationOptions RedisConfiguration { get; set; }
    }
}