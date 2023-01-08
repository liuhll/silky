using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        private readonly IServerLocalInvokerFactory _serverLocalInvokerFactory;

        public DefaultLocalExecutor(IServerLocalInvokerFactory serverLocalInvokerFactory)
        {
            _serverLocalInvokerFactory = serverLocalInvokerFactory;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string? serviceKey = null)
        {
            var instance = EngineContext.Current.ResolveServiceInstance(serviceKey, serviceEntry.ServiceType);
            var localInvoker =
                _serverLocalInvokerFactory.CreateInvoker(new ServiceEntryContext(serviceEntry, parameters, serviceKey,
                    instance));
             await localInvoker.InvokeAsync();
             return localInvoker.Result;
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<ParameterFrom, object> parameters, string serviceKey)
        {
            throw new NotImplementedException();
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters,
            string serviceKey = null)
        {
            throw new NotImplementedException();
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<string, object> parameters, string serviceKey = null)
        {
            throw new NotImplementedException();
        }
    }
}