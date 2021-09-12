using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Threading;

namespace Silky.Core
{
    public class SilkyCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
          
        }

        public int Order { get; } = 0;
    }
}