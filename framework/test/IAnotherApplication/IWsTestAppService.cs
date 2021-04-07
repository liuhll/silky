using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}