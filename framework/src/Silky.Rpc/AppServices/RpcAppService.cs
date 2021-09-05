using System.Collections.Generic;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices
{
    public class RpcAppService : IRpcAppService
    {
        private readonly IHandleSupervisor _handleSupervisor;
        private readonly IRequestServiceSupervisor _requestServiceSupervisor;

        public RpcAppService(IHandleSupervisor handleSupervisor, IRequestServiceSupervisor requestServiceSupervisor)
        {
            _handleSupervisor = handleSupervisor;
            _requestServiceSupervisor = requestServiceSupervisor;
        }

        public GetInstanceSupervisorOutput GetInstanceSupervisor()
        {
            var instanceSupervisorOutput = new GetInstanceSupervisorOutput()
            {
                InvokeInfo = _requestServiceSupervisor.GetServiceInstanceInvokeInfo(),
                HandleInfo = _handleSupervisor.GetServiceInstanceHandleInfo()
            };
            return instanceSupervisorOutput;
        }
        
        public IReadOnlyCollection<ServiceEntryHandleInfo> GetServiceEntryHandleInfos()
        {
            return _handleSupervisor.GetServiceEntryHandleInfos();
        }

        public IReadOnlyCollection<ServiceEntryInvokeInfo> GetServiceEntryInvokeInfos()
        {
            return _requestServiceSupervisor.GetServiceEntryInvokeInfos();
        }
    }
}