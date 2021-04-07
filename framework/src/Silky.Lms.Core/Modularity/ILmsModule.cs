using System.Threading.Tasks;

namespace Silky.Lms.Core.Modularity
{
    public interface ILmsModule
    {
        string Name { get; }
        Task Initialize(ApplicationContext applicationContext);
        Task Shutdown(ApplicationContext applicationContext);
    }
}