using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Core;

namespace Silky.Http.RateLimit
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class RateLimitModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisOptions = configuration.GetRateLimitRedisOptions();
            services.AddClientRateLimit(redisOptions);
            services.AddIpRateLimit(redisOptions);
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseClientRateLimiting();
            application.UseClientRateLimiting();
        }
    }
}