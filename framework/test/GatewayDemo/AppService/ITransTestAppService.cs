using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;

namespace GatewayDemo.AppService
{
    /// <summary>
    /// Trans Test Services
    /// </summary>
    [ServiceRoute(template: "test/{appservice=trans}")]
    public interface ITransTestAppService
    {
        [Transaction]
        Task<string> Delete(TestInput input);
    }
}