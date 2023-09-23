using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        Task PreInitializeModules(IServiceProvider serviceProvider);
        
        Task InitializeModules(IServiceProvider serviceProvider);
        
        Task PostInitializeModules(IServiceProvider serviceProvider);

        Task ShutdownModules(IServiceProvider serviceProvider);
    }
}