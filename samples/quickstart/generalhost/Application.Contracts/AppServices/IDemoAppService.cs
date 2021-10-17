using System.Threading.Tasks;
using Silky.Rpc.Routing;

namespace Application.Contracts.AppServices
{
    [ServiceRoute]
    public interface IDemoAppService
    {
        Task<string> Say(string line);
    }
}