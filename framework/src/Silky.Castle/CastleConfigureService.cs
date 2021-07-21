using Silky.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle.Adapter;

namespace Silky.Castle
{
    public class CastleConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(SilkyAsyncDeterminationInterceptor<>));

        }

        public int Order { get; } = 1;
    }
}