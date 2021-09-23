using Nacos.V2;

namespace Silky.RegistryCenter.Nacos.Listeners
{
    public class ServiceListener : IListener
    {
        private readonly string m_hostName;
        private readonly IServiceProvider _serviceProvider;

        internal ServiceListener(string hostName, IServiceProvider serviceProvider)
        {
            m_hostName = hostName;
            _serviceProvider = serviceProvider;
        }

        public void ReceiveConfigInfo(string configInfo)
        {
            _serviceProvider.UpdateService(m_hostName, configInfo);
        }
    }
}