using Lms.Core.DependencyInjection;

namespace Lms.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        void InitializeModules();
        
        void ShutdownModules();
    }
}