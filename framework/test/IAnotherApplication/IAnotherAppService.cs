using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Transaction;
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