using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Rpc.Diagnostics;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        private static readonly DiagnosticListener s_diagnosticListener =
            new(RpcDiagnosticListenerNames.DiagnosticServerListenerName);

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var instance = EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            parameters = serviceEntry.ConvertParameters(parameters);

            var filters = EngineContext.Current.ResolveAll<IServerFilter>().OrderBy(p => p.Order).ToArray();
            var rpcActionExcutingContext = new ServiceEntryExecutingContext()
            {
                ServiceEntry = serviceEntry,
                Parameters = parameters,
                ServiceKey = serviceKey,
                InstanceType = instance.GetType()
            };

            var rpcActionExecutedContext = new ServiceEntryExecutedContext();
            try
            {
                object result;
                foreach (var filter in filters)
                {
                    filter.OnActionExecuting(rpcActionExcutingContext);
                }

                if (serviceEntry.IsAsyncMethod)
                {
                    result = await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
                }
                else
                {
                    result = serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
                }

                rpcActionExecutedContext.Result = result;

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
            finally
            {
                TracingWrite(new LocalExecuteEventData()
                {
                    ServiceEntryId = serviceEntry.Id,
                    FilterNames = filters.Select(p => p.GetType().FullName).ToArray(),
                    MethodName = serviceEntry.MethodInfo.Name,
                    IsDistributeTrans = serviceEntry.IsTransactionServiceEntry(),
                    Exception = rpcActionExecutedContext.Exception,
                    IsAsyncMethod = serviceEntry.IsAsyncMethod
                });
            }
        }

        private void TracingWrite(LocalExecuteEventData localExecuteEventData)
        {
            if (s_diagnosticListener.IsEnabled(RpcDiagnosticListenerNames.LocalMethodExecute))
            {
                s_diagnosticListener.Write(RpcDiagnosticListenerNames.LocalMethodExecute, localExecuteEventData);
            }
        }
        
    }
}