using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        Task PreInitializeModules();
        
        Task InitializeModules();
        
        Task PostInitializeModules();

        Task ShutdownModules();
    }
}