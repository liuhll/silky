using System.Collections.Generic;
using System.Diagnostics;
using Silky.Core;
using Silky.Rpc.Address;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.AppServices
{
    public class RpcAppService : IRpcAppService
    {
        private readonly IServerHandleSupervisor _serverHandleSupervisor;
        private readonly IRequestServiceSupervisor _requestServiceSupervisor;

        public RpcAppService(IServerHandleSupervisor serverHandleSupervisor, IRequestServiceSupervisor requestServiceSupervisor)
        {
            _serverHandleSupervisor = serverHandleSupervisor;
            _requestServiceSupervisor = requestServiceSupervisor;
        }

        public GetInstanceDetailOutput GetInstanceDetail()
        {
            var instanceSupervisorOutput = new GetInstanceDetailOutput()
            {
                HostName = EngineContext.Current.HostName,
                Address = AddressHelper.GetRpcAddressModel().IPEndPoint.ToString(),
                StartTime = Process.GetCurrentProcess().StartTime,
                InvokeInfo = _requestServiceSupervisor.GetServiceInstanceInvokeInfo(),
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
            return _requestServiceSupervisor.GetServiceEntryInvokeInfos();
        }
    }
}