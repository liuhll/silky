using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices
{
    public class RpcAppService : IRpcAppService
    {
        private readonly IServerHandleSupervisor _serverHandleSupervisor;
        private readonly IInvokeSupervisor _invokeSupervisor;

        public RpcAppService(IServerHandleSupervisor serverHandleSupervisor, IInvokeSupervisor invokeSupervisor)
        {
            _serverHandleSupervisor = serverHandleSupervisor;
            _invokeSupervisor = invokeSupervisor;
        }

        public GetInstanceDetailOutput GetInstanceDetail()
        {
            var instanceSupervisorOutput = new GetInstanceDetailOutput()
            {
                HostName = EngineContext.Current.HostName,
                Address = RpcEndpointHelper.GetLocalRpcEndpointDescriptor().GetHostAddress(),
                StartTime = Process.GetCurrentProcess().StartTime,
                InvokeInfo = _invokeSupervisor.GetServiceInstanceInvokeInfo(),
                HandleInfo = _serverHandleSupervisor.GetServiceInstanceHandleInfo()
            };
            return instanceSupervisorOutput;
        }

        public IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos()
        {
            return _serverHandleSupervisor.GetServiceEntryHandleInfos();
        }

        public IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos()
        {
            return _invokeSupervisor.GetServiceEntryInvokeInfos();
        }

        public Task IsHealth()
        {
            return Task.CompletedTask;
        }
    }
}