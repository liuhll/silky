using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Transaction;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=trans}")]
    public interface ITransTestAppService
    {
        [Transaction]
        Task<string> Delete(string name);
    }
}