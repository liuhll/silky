using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transaction;

namespace IAnotherApplication
{
    [ServiceRoute]
    public interface IAnotherAppService
    {
        [Transaction]
        Task<string> Delete(string name);
        
    
        Task<string> Create(string name);
    }
}