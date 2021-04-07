using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Lms.Transaction;
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
        Task<string> DeleteTwo(string name);
    
        Task<string> Create(string name);
    }
}