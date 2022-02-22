using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Auditing;
using Silky.Rpc.Routing;

namespace IAnotherApplication
{
    [ServiceRoute(ServiceName = "AnotherAppService")]
    [DisableAuditing]
    public interface IAnotherAppService
    {
        [Transaction]
        [HttpDelete("one")]
        [Governance(ProhibitExtranet = true)]
        Task<string> DeleteOne(string name);

        [Transaction]
        [HttpDelete("two")]
        [Governance(ProhibitExtranet = true)]
        Task<string> DeleteTwo(string address);

        Task<string> Create(string name);
        
        void ReturnNullTest();
    }
}