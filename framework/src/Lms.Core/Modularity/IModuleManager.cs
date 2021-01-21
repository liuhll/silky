using JetBrains.Annotations;

namespace Lms.Core.Modularity
{
    public interface IModuleManager
    {
        void InitializeModules();
        
        void ShutdownModules();
    }
}