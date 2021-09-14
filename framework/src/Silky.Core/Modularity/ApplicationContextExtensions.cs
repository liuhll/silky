using System.Linq;

namespace Silky.Core.Modularity
{
    public static class ApplicationContextExtensions
    {
        public static bool IsDependsOnModule(this ApplicationContext applicationContext, string moduleName)
        {
            return applicationContext.ModuleContainer.Modules.Any(p => p.Name == moduleName);
        }
    }
}