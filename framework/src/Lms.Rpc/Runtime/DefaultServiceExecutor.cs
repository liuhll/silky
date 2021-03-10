using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Runtime
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return await serviceEntry.Executor(serviceKey, parameters);
        }
    }
}