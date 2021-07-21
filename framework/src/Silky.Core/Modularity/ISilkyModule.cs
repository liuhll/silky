using System.Threading.Tasks;

namespace Silky.Core.Modularity
{
    public interface ISilkyModule
    {
        string Name { get; }
        Task Initialize(ApplicationContext applicationContext);
        Task Shutdown(ApplicationContext applicationContext);
    }
}