using System.Threading.Tasks;
using Silky.Rpc.Routing;

namespace Application.Contracts.AppServices
{
    [ServiceRoute]
    public interface IGreetingAppService
    {
        Task<string> Say(string line);
    }
}