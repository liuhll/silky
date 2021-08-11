using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace IAnotherApplication
{
    [ServiceRoute(ServiceName = "AnotherAppService")]
    public interface IAnotherAppService
    {
        [Transaction]
        [HttpDelete("one")]
        [Governance(ProhibitExtranet = true, ExecutionTimeout = -1)]
        Task<string> DeleteOne(string name);

        [Transaction]
        [HttpDelete("two")]
        [Governance(ProhibitExtranet = true, ExecutionTimeout = -1)]
        Task<string> DeleteTwo(string address);

        Task<string> Create(string name);
    }
}