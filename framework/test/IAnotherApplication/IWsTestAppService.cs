using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IWsTestAppService
    {
        Task Echo(string businessId,string msg);
    }
}