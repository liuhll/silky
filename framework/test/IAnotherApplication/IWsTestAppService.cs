using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}