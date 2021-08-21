using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Address.HealthCheck
{
    public interface IGatewayHealthCheck : ISingletonDependency
    {
        void Monitor(IAddressModel addressModel);

        public event RemoveAddressEvent OnRemveAddress;
        event UnhealthEvent OnUnhealth;

        public event AddMonitorEvent OnAddMonitor;
    }
}