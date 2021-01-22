using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        Task InitializeModules();
        
        Task ShutdownModules();
    }
}