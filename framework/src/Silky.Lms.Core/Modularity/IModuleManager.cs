using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        Task InitializeModules();
        
        Task ShutdownModules();
    }
}