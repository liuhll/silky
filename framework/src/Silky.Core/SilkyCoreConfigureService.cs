using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Threading;

namespace Silky.Core
{
    public class SilkyCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHostedService<InitSilkyHostedService>();
            services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
        }

        public int Order { get; } = 0;
    }
}