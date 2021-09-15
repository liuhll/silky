using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IConfigureService
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}