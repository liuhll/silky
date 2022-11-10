using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface ISilkyModule
    {
        string Name { get; }
        
        Task PreInitialize(ApplicationInitializationContext context);
        
        Task Initialize(ApplicationInitializationContext context);
        
        Task PostInitialize(ApplicationInitializationContext context);
        
        Task Shutdown(ApplicationShutdownContext context);

        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
      
    }
}