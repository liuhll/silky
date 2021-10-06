using System.Linq;
using System.Threading.Tasks;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        private readonly ServerFilterProvider _serverFilterProvider;

        public DefaultLocalExecutor(ServerFilterProvider serverFilterProvider)
        {
            _serverFilterProvider = serverFilterProvider;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var instance = EngineContext.Current.ResolveServiceInstance(serviceKey, serviceEntry.ServiceType);
            parameters = serviceEntry.ConvertParameters(parameters);

            var filters = _serverFilterProvider.GetServerFilters(serviceEntry, instance.GetType());
            var rpcActionExecutingContext = new ServerExecutingContext()
            {
                ServiceEntry = serviceEntry,
                Parameters = parameters,
                ServiceKey = serviceKey,
                InstanceType = instance.GetType()
            };

            object result;

            foreach (var filter in filters)
            {
                filter.OnActionExecuting(rpcActionExecutingContext);
            }

            if (serviceEntry.IsAsyncMethod)
            {
                result = await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
            }
            else
            {
                result = serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
            }

            var rpcActionExecutedContext = new ServerExecutedContext()
            {
                Result = result
            };

            foreach (var filter in filters)
            {
                filter.OnActionExecuted(rpcActionExecutedContext);
            }

            if (rpcActionExecutedContext.Exception != null)
            {
                throw rpcActionExecutedContext.Exception;
            }

            return rpcActionExecutedContext.Result;
        }
    }
}