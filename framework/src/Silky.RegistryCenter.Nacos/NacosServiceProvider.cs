using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.RegistryCenter.Nacos.Listeners;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServiceProvider : IServiceProvider
    {
        private readonly ISerializer _serializer;
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;
        private readonly ServiceListener _serviceListener;
        private readonly INacosConfigService _nacosConfigService;
        private ServiceDescriptor[] m_services = null;


        public NacosServiceProvider(ISerializer serializer,
            IOptionsMonitor<NacosRegistryCenterOptions> nacosRegistryCenterOptions,
            ServiceListener serviceListener,
            INacosConfigService nacosConfigService)
        {
            _serializer = serializer;
            _serviceListener = serviceListener;
            _nacosConfigService = nacosConfigService;
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.CurrentValue;
            m_services = GetServices(EngineContext.Current.HostName, _nacosRegistryCenterOptions.GroupName).GetAwaiter()
                .GetResult();
        }

        public async Task<ServiceDescriptor[]> GetServices(string hostName, string group, long timeoutMs = 1000)
        {
            if (m_services != null)
            {
                return m_services;
            }

            var serviceConfigValue =
                await _nacosConfigService.GetConfigAndSignListener(hostName, group, timeoutMs, _serviceListener);
            var services = _serializer.Deserialize<ServiceDescriptor[]>(serviceConfigValue);
            return services;
        }

        public async Task PublishServices(string hostName, string @group, ServiceDescriptor[] serviceDescriptors,
            long timeoutMs = 1000)
        {
            if (m_services == null)
            {
                return;
            }

            var serviceConfigValue = _serializer.Serialize(serviceDescriptors);
            var result = await _nacosConfigService.PublishConfig(hostName, group, serviceConfigValue);
            if (!result)
            {
                throw new SilkyException("Failed to publish service description information");
            }
        }
    }
}