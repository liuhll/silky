using System;
using System.Collections.Generic;
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

            try
            {
                if (serviceEntry.IsAsyncMethod)
                {
                    result = await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
                }
                else
                {
                    result = serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
                }
            }
            catch (Exception ex)
            {
                foreach (var filter in filters)
                {
                    filter.OnActionException(new ServerExceptionContext()
                    {
                        Exception = ex,
                        ServiceEntry = serviceEntry,
                    });
                }
                throw;
            }
            
            var rpcActionExecutedContext = new ServerExecutedContext()
            {
                Result = result,
                ServiceEntry = serviceEntry,
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

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<ParameterFrom, object> parameters, string serviceKey)
        {
            throw new NotImplementedException();
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters, string serviceKey = null)
        {
            throw new NotImplementedException();
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<string, object> parameters, string serviceKey = null)
        {
            throw new NotImplementedException();
        }
    }
}