using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime
{
    public class DefaultExecutor : IExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return await serviceEntry.Executor(serviceKey, parameters);
        }
    }
}