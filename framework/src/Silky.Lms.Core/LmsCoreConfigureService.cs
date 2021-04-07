using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Core.Threading;

namespace Silky.Lms.Core
{
    public class LmsCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHostedService<InitLmsHostedService>();
            services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance);
        }

        public int Order { get; } = 0;
    }
}