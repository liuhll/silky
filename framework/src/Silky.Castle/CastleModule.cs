using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle.Adapter;
using Silky.Core.Modularity;

namespace Silky.Castle
{
    public class CastleModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(SilkyAsyncDeterminationInterceptor<>));
        }
    }
}