using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace IAnotherApplication
{
    [ServiceRoute(ServiceProtocol = ServiceProtocol.Ws)]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}