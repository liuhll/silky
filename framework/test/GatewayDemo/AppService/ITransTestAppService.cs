using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transaction;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=trans}")]
    public interface ITransTestAppService
    {
        [Transaction]
        Task<string> Create(string name);
    }
}