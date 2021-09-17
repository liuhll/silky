using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;

namespace IAnotherApplication
{
    [ServiceRoute(ServiceName = "AnotherAppService")]
    public interface IAnotherAppService
    {
        [Transaction]
        [HttpDelete("one")]
        [Governance(ProhibitExtranet = true, TimeoutMillSeconds = -1)]
        Task<string> DeleteOne(string name);

        [Transaction]
        [HttpDelete("two")]
        [Governance(ProhibitExtranet = true, TimeoutMillSeconds = -1)]
        Task<string> DeleteTwo(string address);

        Task<string> Create(string name);
    }
}