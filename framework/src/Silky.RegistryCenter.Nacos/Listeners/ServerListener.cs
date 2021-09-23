using System.Threading.Tasks;
using Nacos.V2;
using Nacos.V2.Naming.Event;

namespace Silky.RegistryCenter.Nacos.Listeners
{
    public class ServerListener : IEventListener
    {
        private readonly NacosServerRegister _serverRegister;

        public ServerListener(NacosServerRegister serverRegister)
        {
            _serverRegister = serverRegister;
        }

        public async Task OnEvent(IEvent @event)
        {
            var instancesChangeEvent = @event as InstancesChangeEvent;
            if (instancesChangeEvent == null)
            {
                return;
            }

            await _serverRegister.CreateServerListener(instancesChangeEvent.ServiceName);
            await _serverRegister.UpdateServer(instancesChangeEvent.ServiceName, instancesChangeEvent.Hosts);
        }
    }
}