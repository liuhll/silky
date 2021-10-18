using System.Threading.Tasks;
using Silky.Rpc.Routing;

namespace WebHostDemo.AppServices
{
    [ServiceRoute]
    public interface IGreetingAppService
    {   
        Task<string> Get();
    }
}