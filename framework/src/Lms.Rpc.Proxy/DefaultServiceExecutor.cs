using System.Threading.Tasks;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Proxy
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return serviceEntry.Executor(serviceKey, parameters);
        }
    }
}