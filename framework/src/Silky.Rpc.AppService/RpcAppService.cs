using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppService
{
    public class RpcAppService : IRpcAppService
    {
        public async Task<ServerInstanceDetailInfo> GetInstanceDetail()
        {
            var serverHandleSupervisor = EngineContext.Current.Resolve<IServerHandleMonitor>();
            var invokeSupervisor = EngineContext.Current.Resolve<IInvokeMonitor>();
            var instanceSupervisorOutput = new ServerInstanceDetailInfo()
            {
                HostName = EngineContext.Current.HostName,
                Address = SilkyEndpointHelper.GetLocalRpcEndpoint().GetAddress(),
                StartTime = Process.GetCurrentProcess().StartTime,
            };
            if (serverHandleSupervisor != null && invokeSupervisor != null)
            {
                instanceSupervisorOutput.InvokeInfo = await invokeSupervisor.GetServerInstanceInvokeInfo();
                instanceSupervisorOutput.HandleInfo = await serverHandleSupervisor.GetServerInstanceHandleInfo();
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

            return Array.Empty<ServerHandleInfo>();
        }

        public async Task<IReadOnlyCollection<ClientInvokeInfo>> GetServiceEntryInvokeInfos()
        {
            var invokeSupervisor = EngineContext.Current.Resolve<IInvokeMonitor>();
            if (invokeSupervisor != null)
            {
                return await invokeSupervisor.GetServiceEntryInvokeInfos();
            }

            return Array.Empty<ClientInvokeInfo>();
        }

        public Task<bool> IsHealth()
        {
            return Task.FromResult(true);
        }
    }
}