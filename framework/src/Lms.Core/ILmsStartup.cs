using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core
{
    public interface ILmsStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}