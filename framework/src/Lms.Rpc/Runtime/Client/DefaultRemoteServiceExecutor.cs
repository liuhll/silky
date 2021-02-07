using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Messages;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceExecutor : IRemoteServiceExecutor
    {
        private readonly IRemoteServiceInvoker _remoteServiceInvoker;
        private readonly IServiceEntryLocate _serviceEntryLocate;
        public DefaultRemoteServiceExecutor(IRemoteServiceInvoker remoteServiceInvoker, 
            IServiceEntryLocate serviceEntryLocate)
        {
            _remoteServiceInvoker = remoteServiceInvoker;
            _serviceEntryLocate = serviceEntryLocate;
        }

        public async Task<object> Execute(string serviceId, IList<object> parameters)
        {
            var serviceEntry = _serviceEntryLocate.GetServiceEntryById(serviceId);
            if (serviceEntry.IsLocal)
            {
                return serviceEntry.Executor(null, parameters);
            }

            return await Execute(serviceEntry, parameters);
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, IList<object> parameters)
        {
            // todo 1. 失败重试 2. 缓存拦截 3.确定返回值 
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceDescriptor.Id,
                Parameters = parameters,
            };
            
            var invokeResult = await _remoteServiceInvoker.Invoke(remoteInvokeMessage);
            return invokeResult.Result;

        }
    }
}