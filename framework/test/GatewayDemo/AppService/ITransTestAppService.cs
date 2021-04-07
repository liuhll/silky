using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Lms.Transaction;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=trans}")]
    public interface ITransTestAppService
    {
        [Transaction]
        Task<string> Delete(string name);
    }
}