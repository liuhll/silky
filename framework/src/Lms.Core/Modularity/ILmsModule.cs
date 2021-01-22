using System.Threading.Tasks;

namespace Lms.Core.Modularity
{
    public interface ILmsModule
    {
        Task Initialize(ApplicationContext applicationContext);
        Task Shutdown(ApplicationContext applicationContext);
    }
}