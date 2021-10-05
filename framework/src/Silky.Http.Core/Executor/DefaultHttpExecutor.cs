using System.Threading.Tasks;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Executor
{
    public class DefaultHttpExecutor : IHttpExecutor
    {
        private readonly IExecutor _executor;

        public DefaultHttpExecutor(IExecutor executor)
        {
            _executor = executor;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return await _executor.Execute(serviceEntry, parameters, serviceKey);
        }
    }
}