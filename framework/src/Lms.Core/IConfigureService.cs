using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core
{
    public interface IConfigureService
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
        int Order { get; }
    }
}