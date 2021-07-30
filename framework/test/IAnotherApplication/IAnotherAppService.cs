using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace IAnotherApplication
{
    [ServiceRoute]
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
    }
}