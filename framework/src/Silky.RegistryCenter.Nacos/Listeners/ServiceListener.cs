using Nacos.V2;
using Silky.Core.DependencyInjection;

namespace Silky.RegistryCenter.Nacos.Listeners
{
    public class ServiceListener : IListener, ISingletonDependency
    {
        public void ReceiveConfigInfo(string configInfo)
        {
        }
    }
}