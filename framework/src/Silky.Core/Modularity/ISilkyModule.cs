using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface ISilkyModule
    {
        string Name { get; }
        Task Initialize(ApplicationContext applicationContext);
        Task Shutdown(ApplicationContext applicationContext);

        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}