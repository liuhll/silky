using System.Linq;
using System.Threading.Tasks;
using Lms.Core;

namespace Lms.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var instance = EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            parameters = serviceEntry.ConvertParameters(parameters);
            if (serviceEntry.IsAsyncMethod)
            {
                return await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
            }

            return serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
        }
    }
}