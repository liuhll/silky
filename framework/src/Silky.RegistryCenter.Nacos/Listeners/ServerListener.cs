using System.Threading;
using System.Threading.Tasks;
using Nacos.V2;
using Nacos.V2.Naming.Event;
using Silky.Core.Threading;

namespace Silky.RegistryCenter.Nacos.Listeners
{
    public class ServerListener : IEventListener
    {
        private readonly NacosServerRegister _serverRegister;

        protected SemaphoreSlim SyncSemaphore { get; }

        public ServerListener(NacosServerRegister serverRegister)
        {
            _serverRegister = serverRegister;
            SyncSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task OnEvent(IEvent @event)
        {
            var instancesChangeEvent = @event as InstancesChangeEvent;
            if (instancesChangeEvent == null)
            {
                return;
            }
            using (await SyncSemaphore.LockAsync())
            {
                await _serverRegister.UpdateServer(instancesChangeEvent.ServiceName, instancesChangeEvent.GroupName,
                    instancesChangeEvent.Hosts);
            }
        }
    }
}