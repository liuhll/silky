using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices
{
    public class RpcAppService : IRpcAppService
    {
        public async Task<GetInstanceDetailOutput> GetInstanceDetail()
        {
            var serverHandleSupervisor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var invokeSupervisor = EngineContext.Current.Resolve<IInvokeMonitor>();
            var instanceSupervisorOutput = new GetInstanceDetailOutput()
            {
                HostName = EngineContext.Current.HostName,
                Address = RpcEndpointHelper.GetLocalTcpEndpoint().GetAddress(),
                StartTime = Process.GetCurrentProcess().StartTime,
            };
            if (serverHandleSupervisor != null && invokeSupervisor != null)
            {
                instanceSupervisorOutput.InvokeInfo = await invokeSupervisor.GetServiceInstanceInvokeInfo();
                instanceSupervisorOutput.HandleInfo = await serverHandleSupervisor.GetServiceInstanceHandleInfo();
            }

            return instanceSupervisorOutput;
        }

        public async Task<IReadOnlyCollection<ServerHandleInfo>> GetServiceEntryHandleInfos()
        {
            var serverHandleSupervisor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            if (serverHandleSupervisor != null)
            {
                return await serverHandleSupervisor.GetServiceEntryHandleInfos();
            }

            return null;
        }

        public Task<IReadOnlyCollection<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos()
        {
            var invokeSupervisor = EngineContext.Current.Resolve<IInvokeMonitor>();
            if (invokeSupervisor != null)
            {
                return invokeSupervisor.GetServiceEntryInvokeInfos();
            }

            return null;
        }

        public Task<bool> IsHealth()
        {
            return Task.FromResult(true);
        }
    }
}