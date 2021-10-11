using System.Threading.Tasks;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IWsTestAppService
    {
        Task Echo(string businessId, string msg);
    }
}