using Lms.Core;
using Lms.Lock.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Lock
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