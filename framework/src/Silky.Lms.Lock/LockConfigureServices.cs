using Silky.Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Lock.Configuration;

namespace Silky.Lms.Lock
{
    public class LockConfigureServices : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LockOptions>()
                .Bind(configuration.GetSection(LockOptions.Lock));
        }

        public int Order { get; } = 2;
    }
}