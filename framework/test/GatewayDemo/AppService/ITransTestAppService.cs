using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Transaction;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=trans}")]
    public interface ITransTestAppService
    {
        [Transaction]
        Task<string> Delete(TestInput input);
    }
}