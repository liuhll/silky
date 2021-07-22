using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Lock.Configuration;

namespace Silky.Lock
{
    public class LockModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LockOptions>()
                .Bind(configuration.GetSection(LockOptions.Lock));
        }
    }
}