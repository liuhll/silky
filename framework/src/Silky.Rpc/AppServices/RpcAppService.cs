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

        public GetServiceEntrySupervisorOutput GetServiceEntrySupervisor(string serviceId)
        {
            var serviceEntrySupervisorOutput = new GetServiceEntrySupervisorOutput()
            {
                ServiceEntryHandleInfo = _handleSupervisor.GetServiceHandleInfo(serviceId),
                ServiceEntryInvokeInfo = _requestServiceSupervisor.GetServiceInvokeInfo(serviceId)
            };
            return serviceEntrySupervisorOutput;
        }
    }
}