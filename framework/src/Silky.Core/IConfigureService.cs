using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Core
{
    public interface IConfigureService
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}