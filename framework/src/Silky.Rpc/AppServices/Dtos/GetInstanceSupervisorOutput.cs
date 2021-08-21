using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.AppServices.Dtos
{
    public class GetInstanceSupervisorOutput
    {
        public ServiceInstanceHandleInfo HandleInfo { get; set; }

        public ServiceInstanceInvokeInfo InvokeInfo { get; set; }
    }
}