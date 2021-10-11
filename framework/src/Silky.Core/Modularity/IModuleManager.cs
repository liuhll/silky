using System.Threading.Tasks;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Modularity
{
    public interface IModuleManager : ISingletonDependency
    {
        Task InitializeModules();

        Task ShutdownModules();
    }
}