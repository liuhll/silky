using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IAnotherAppService
    {
        [Transaction]
        [HttpDelete("one")]
        Task<string> DeleteOne(string name);
        
        [Transaction]
        [HttpDelete("two")]
        Task<string> DeleteTwo(string name);
    
        Task<string> Create(string name);
    }
}