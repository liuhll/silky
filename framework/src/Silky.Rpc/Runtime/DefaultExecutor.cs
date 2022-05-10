using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime
{
    public class DefaultExecutor : IExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return await serviceEntry.Executor(serviceKey, parameters);
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<ParameterFrom, object> parameters, string serviceKey)
        {
            var remoteExecutor = EngineContext.Current.Resolve<IRemoteExecutor>();
            return remoteExecutor.Execute(serviceEntryDescriptor, parameters, serviceKey);
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters, string serviceKey = null)
        {
            var remoteExecutor = EngineContext.Current.Resolve<IRemoteExecutor>();
            return remoteExecutor.Execute(serviceEntryDescriptor, parameters, serviceKey);
        }

        public Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, IDictionary<string, object> parameters, string serviceKey = null)
        {
            var remoteExecutor = EngineContext.Current.Resolve<IRemoteExecutor>();
            return remoteExecutor.Execute(serviceEntryDescriptor, parameters, serviceKey);
        }
    }
}